using System.Reflection;
using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public int ammoMagazinesToAdd = 1; // cuántos cargadores agrega

    private void OnTriggerEnter(Collider other)
    {
        // Buscamos el PlayerWeapon en el collider o en sus padres/hijos
        PlayerWeapon pw = other.GetComponent<PlayerWeapon>()
                          ?? other.GetComponentInParent<PlayerWeapon>()
                          ?? other.GetComponentInChildren<PlayerWeapon>();

        if (pw == null) return;

        // 1) Intentar método público AddMagazines(int) o AddAmmo(int)
        MethodInfo mi = pw.GetType().GetMethod("AddMagazines", BindingFlags.Public | BindingFlags.Instance);
        if (mi == null)
            mi = pw.GetType().GetMethod("AddAmmo", BindingFlags.Public | BindingFlags.Instance);

        if (mi != null)
        {
            // llamar al método (si espera 1 parámetro int)
            ParameterInfo[] pars = mi.GetParameters();
            if (pars.Length == 1 && (pars[0].ParameterType == typeof(int) || pars[0].ParameterType == typeof(float)))
            {
                object arg = pars[0].ParameterType == typeof(int) ? (object)ammoMagazinesToAdd : (object)(float)ammoMagazinesToAdd;
                mi.Invoke(pw, new object[] { arg });
                Destroy(gameObject);
                return;
            }
        }

        // 2) Intentar incrementar un campo público llamado extraMagazines / currentMagazines / magazines
        FieldInfo fi = pw.GetType().GetField("extraMagazines", BindingFlags.Public | BindingFlags.Instance)
                     ?? pw.GetType().GetField("currentMagazines", BindingFlags.Public | BindingFlags.Instance)
                     ?? pw.GetType().GetField("magazines", BindingFlags.Public | BindingFlags.Instance);

        if (fi != null)
        {
            if (fi.FieldType == typeof(int))
            {
                int cur = (int)fi.GetValue(pw);
                fi.SetValue(pw, cur + ammoMagazinesToAdd);
                Destroy(gameObject);
                return;
            }
            if (fi.FieldType == typeof(float))
            {
                float cur = (float)fi.GetValue(pw);
                fi.SetValue(pw, cur + ammoMagazinesToAdd);
                Destroy(gameObject);
                return;
            }
        }

        // 3) Intentar incrementar currentAmmo si el diseño es distinto (fallback)
        FieldInfo fiAmmo = pw.GetType().GetField("currentAmmo", BindingFlags.Public | BindingFlags.Instance)
                        ?? pw.GetType().GetField("ammo", BindingFlags.Public | BindingFlags.Instance);
        if (fiAmmo != null && fiAmmo.FieldType == typeof(int))
        {
            int cur = (int)fiAmmo.GetValue(pw);
            // asumimos que un "cargador" equivale a magazineSize si existe
            FieldInfo fiMagSize = pw.GetType().GetField("magazineSize", BindingFlags.Public | BindingFlags.Instance)
                             ?? pw.GetType().GetField("maxAmmoPerMag", BindingFlags.Public | BindingFlags.Instance);

            int addBullets = 0;
            if (fiMagSize != null && fiMagSize.FieldType == typeof(int))
                addBullets = (int)fiMagSize.GetValue(pw) * ammoMagazinesToAdd;

            // si no existe magazineSize, simplemente sumamos ammoMagazinesToAdd
            fiAmmo.SetValue(pw, cur + (addBullets > 0 ? addBullets : ammoMagazinesToAdd));
            Destroy(gameObject);
            return;
        }

        // 4) Si llegamos acá, no supimos cómo darle ammo: log para depuración
        Debug.LogWarning($"AmmoPickup: no pude aplicar ammo al PlayerWeapon ({pw.GetType().Name}). Añadí un método public AddAmmo/AddMagazines o un campo 'extraMagazines' para compatibilidad.");
    }
}
