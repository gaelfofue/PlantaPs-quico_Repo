using UnityEngine;

public class SimpleSpikeDeath : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Buscar CUALQUIER componente que tenga el método InstantDeath
            MonoBehaviour[] scripts = other.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour script in scripts)
            {
                // Usar reflexión para buscar el método
                var method = script.GetType().GetMethod("InstantDeath");
                if (method != null)
                {
                    method.Invoke(script, null); // Llamar al método
                    return; // Salir después de encontrar y llamar
                }
            }

            // Si no se encuentra ningún método InstantDeath, mostrar error
            Debug.LogError("No se encontró ningún script con el método InstantDeath() en el jugador");
        }
    }
}