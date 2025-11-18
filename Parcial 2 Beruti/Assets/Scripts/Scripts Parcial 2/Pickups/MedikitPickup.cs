using System.Reflection;
using UnityEngine;

public class MedikitPickup : MonoBehaviour
{
    public int healAmount = 30;

    private void OnTriggerEnter(Collider other)
    {
        // Buscar PlayerHealth en el collider o en sus padres/hijos
        PlayerHealth ph = other.GetComponent<PlayerHealth>()
                            ?? other.GetComponentInParent<PlayerHealth>()
                            ?? other.GetComponentInChildren<PlayerHealth>();

        if (ph == null) return;

        // 1) Intentar método público Heal(int/float)
        MethodInfo mi = ph.GetType().GetMethod("Heal", BindingFlags.Public | BindingFlags.Instance);
        if (mi != null)
        {
            ParameterInfo[] pars = mi.GetParameters();
            if (pars.Length == 1 && pars[0].ParameterType == typeof(int))
            {
                mi.Invoke(ph, new object[] { healAmount });
                Destroy(gameObject);
                return;
            }
            else if (pars.Length == 1 && pars[0].ParameterType == typeof(float))
            {
                mi.Invoke(ph, new object[] { (float)healAmount });
                Destroy(gameObject);
                return;
            }
        }

        // 2) Si no hay método, intentar incrementar currentHealth directamente (campo público)
        FieldInfo fi = ph.GetType().GetField("currentHealth", BindingFlags.Public | BindingFlags.Instance)
                     ?? ph.GetType().GetField("health", BindingFlags.Public | BindingFlags.Instance);

        if (fi != null && (fi.FieldType == typeof(int) || fi.FieldType == typeof(float)))
        {
            // Determinar maxHealth de forma segura (float o int)
            float maxH = 100f;
            FieldInfo fiMax = ph.GetType().GetField("maxHealth", BindingFlags.Public | BindingFlags.Instance)
                            ?? ph.GetType().GetField("maxHP", BindingFlags.Public | BindingFlags.Instance)
                            ?? ph.GetType().GetField("maxHealthValue", BindingFlags.Public | BindingFlags.Instance);

            if (fiMax != null)
            {
                object maxVal = fiMax.GetValue(ph);
                if (maxVal is float mf) maxH = mf;
                else if (maxVal is int miVal) maxH = miVal;
                else
                {
                    float parsed;
                    if (float.TryParse(maxVal?.ToString(), out parsed)) maxH = parsed;
                }
            }

            // Actualizar dependiendo del tipo de campo
            if (fi.FieldType == typeof(int))
            {
                int cur = (int)fi.GetValue(ph);
                int newVal = Mathf.Min(cur + healAmount, Mathf.RoundToInt(maxH));
                fi.SetValue(ph, newVal);
            }
            else // float
            {
                float cur = (float)fi.GetValue(ph);
                float newVal = Mathf.Min(cur + healAmount, maxH);
                fi.SetValue(ph, newVal);
            }

            // Intentamos además llamar UpdateHealthUI si existe
            MethodInfo updateMI = ph.GetType().GetMethod("UpdateHealthUI", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (updateMI != null)
            {
                try { updateMI.Invoke(ph, null); } catch { /* no critical */ }
            }

            Destroy(gameObject);
            return;
        }

        // 3) fallback log
        Debug.LogWarning("MedikitPickup: no pude aplicar heal. Asegurate que PlayerHealth tenga un método público Heal(int/float) o un campo currentHealth.");
    }
}


