using UnityEngine;

public class GolemCollider : MonoBehaviour {
	public Golems.GolemController Golem;

	private void OnParticleCollision(GameObject other) {
		var damager = other.GetComponent<GolemDamagerObject>();
		Debug.Log($"Making damage to the golem. Damager: {damager.name}");
		if(Golem) Golem.TakeDamage(10);
	}
}