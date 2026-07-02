using UnityEngine;
using TMPro;

// The daylight budget for one scavenging run. Lives in the store scene: starts
// counting down as soon as the scene loads, and shows the remaining time top-right
// (it builds its own canvas, same pattern as PlayerHUD, so nothing is wired in the
// editor). When it hits zero, night falls: extra zombies spawn and the early-return
// bond bump is forfeited (TransitionDoor checks NightFell before granting it).
public class DaylightTimer : MonoBehaviour
{
    public static DaylightTimer Instance { get; private set; }

    [Tooltip("Where the extra night zombies appear. Leave empty to skip spawning.")]
    [SerializeField] Transform[] nightSpawnPoints;

    [Tooltip("Zombie prefab spawned at night. If unset, an existing scene zombie is cloned.")]
    [SerializeField] GameObject zombiePrefab;

    float remaining;
    bool nightFell;
    TextMeshProUGUI label;

    public float RemainingSeconds => remaining;
    public bool NightFell => nightFell;

    void Awake()
    {
        Instance = this;
        remaining = GameManager.DAYLIGHT_SECONDS;
        buildLabel();
    }

    void Start()
    {
        // a fresh run means the previous run's night is over
        GameState.Instance?.clearFlag(GameManager.FLAG_NIGHT_FELL);
        updateLabel();
    }

    void Update()
    {
        if (nightFell) return;

        remaining -= Time.deltaTime;
        if (remaining <= 0f) {
            remaining = 0f;
            fallNight();
        }

        updateLabel();
    }

    void fallNight()
    {
        nightFell = true;
        GameState.Instance?.setFlag(GameManager.FLAG_NIGHT_FELL);
        spawnNightZombies();
    }

    void spawnNightZombies()
    {
        if (nightSpawnPoints == null || nightSpawnPoints.Length == 0) return;

        GameObject prefab = zombiePrefab;
        if (prefab == null) {
            // fall back to cloning a zombie already placed in the scene
            Zombie existing = FindAnyObjectByType<Zombie>();
            if (existing == null) {
                Debug.LogWarning("[DaylightTimer] Night fell but there is no zombie prefab or scene zombie to clone.");
                return;
            }
            prefab = existing.gameObject;
        }

        for (int i = 0; i < GameManager.NIGHT_EXTRA_ZOMBIES; i++) {
            Transform point = nightSpawnPoints[i % nightSpawnPoints.Length];
            Instantiate(prefab, point.position, point.rotation);
        }
    }

    // ---------------------------------------------------------------- ui

    void buildLabel()
    {
        UiFactory.ensureEventSystem();
        Canvas canvas = UiFactory.overlayCanvas(transform, "DaylightCanvas");

        label = UiFactory.text(canvas.transform, "TimeLeft", "", 36, Color.white,
                               TextAlignmentOptions.TopRight);
        UiFactory.outline(label);

        var rt = label.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-40f, -30f);
        rt.sizeDelta = new Vector2(320f, 48f);
    }

    void updateLabel()
    {
        if (label == null) return;

        if (nightFell) {
            label.text = "NIGHT";
            label.color = new Color(0.55f, 0.35f, 0.9f, 1f);
            return;
        }

        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        label.text = $"{minutes}:{seconds:00}";

        // drift from white toward red over the final minute
        if (remaining < 60f)
            label.color = Color.Lerp(new Color(0.9f, 0.2f, 0.15f, 1f), Color.white, remaining / 60f);
    }
}
