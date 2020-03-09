using UnityEngine;

public class GunTester : MonoBehaviour {
	public ParticleSystem Particles;
	public bool KeepShooting;

	private void Update() {
		if (KeepShooting) {
			Particles.Play();
			return;
		}

		var fire = Input.GetAxis("Fire1");

		if (fire > 0f) {
			Particles.Play();
		} else {
			Particles.Stop();
		}
	}
}