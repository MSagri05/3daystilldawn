using UnityEngine;

// Visual half of the day/night cycle: dims the assigned "daylight" lights
// (skylight shafts, entrance glow, ambient fill) as the DaylightTimer runs down,
// going dark when night falls — so the store itself tells the time. Each light's
// authored HDRP intensity is cached as its full-morning value and scaled by a
// curve factor; nothing is overwritten, so lumen values stay Inspector-owned.
// Lives in the store scene next to the DaylightTimer. Flashlight, fixtures and
// the safe room are untouched.
public class DaylightVisuals : MonoBehaviour
{
    [Tooltip("Lights that represent daylight. Their current intensity = the full-morning look.")]
    [SerializeField] Light[] daylightLights;

    [Tooltip("Brightness factor across the day: time 0 = morning, 1 = nightfall.")]
    [SerializeField] AnimationCurve intensityOverDay = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Tooltip("Factor once night has fallen (0 = fully dark).")]
    [SerializeField] float nightFactor = 0f;

    float[] authoredIntensity;

    void Awake()
    {
        if (daylightLights == null) return;

        authoredIntensity = new float[daylightLights.Length];
        for (int i = 0; i < daylightLights.Length; i++)
            if (daylightLights[i] != null)
                authoredIntensity[i] = daylightLights[i].intensity;
    }

    void Update()
    {
        DaylightTimer timer = DaylightTimer.Instance;
        if (timer == null || daylightLights == null) return;

        float factor;
        if (timer.NightFell) {
            factor = nightFactor;
        } else {
            float progress = 1f - Mathf.Clamp01(timer.RemainingSeconds / GameManager.DAYLIGHT_SECONDS);
            factor = intensityOverDay.Evaluate(progress);
        }

        for (int i = 0; i < daylightLights.Length; i++)
            if (daylightLights[i] != null)
                daylightLights[i].intensity = authoredIntensity[i] * factor;
    }
}
