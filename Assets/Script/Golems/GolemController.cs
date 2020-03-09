using GolemsEnums;
using System.Collections;
using UnityEngine;

namespace Golems {
	[RequireComponent(typeof(HealthBarController))]
	public abstract class GolemController : MonoBehaviour {
		[SerializeField] protected bool IsInoffensiveGolem = false;
		protected bool CanMove = true;
		[SerializeField] private Animator animator;
		[SerializeField] private float walkSpeed;
		[SerializeField] private float maxCloseDistance;
		[SerializeField] private float maxChasingRange;
		[SerializeField] private float attackCooldownTime;

		[SerializeField] private GameObject healthBarHudItem;
		[SerializeField] private GameObject winCanvas;

		protected bool canAttack = true;
		[SerializeField] protected bool takingDamage = false;
		protected bool isSpecialAttackTriggered;

		protected bool IsChasing => Vector3.Distance(player.transform.position, transform.position) < maxChasingRange;

		protected HealthBarController healthBarController;

		[SerializeField] protected PlayerController player;

		protected bool isDead = false;
		protected bool animDeadPlayed = false;

		//[SerializeField]
		//private Rigidbody rb;
		[SerializeField] protected BoxCollider collision;

		[SerializeField] protected GameObject shield;

		#region Boss Stats

		[SerializeField] protected int maxLife;

		protected int _health;

		public int Health {
			get { return _health; }
			private set {
				_health = value;
				healthBarController.UpdateBar(_health);
			}
		}

		[SerializeField] private float damage;
		[SerializeField] private float cooldownGetDamage;

		#endregion

		//Esta parte seguramente se daba poner en el script del golem y no en el padre

		#region Animations state names

		protected readonly string IDLE_STATE_NAME = "Idle1";
		protected readonly string WALK_STATE_NAME = "Walk";
		protected readonly string DEATH_STATE_NAME = "Death1";
		protected readonly string TAKE_DAMAGE_STATE_NAME = "GetHit2";
		protected readonly string NORMAL_ATTACK_STATE_NAME = "Attack3";
		protected readonly string SPECIAL_ATTACK_STATE_NAME = "Attack2";

		#endregion

		#region Animation Parameters

		protected readonly string DEATH_TRIGGER = "Death1";
		protected readonly string SPECIAL_ATTACK_TRIGGER = "Attack2";
		protected readonly string ATTACK_TRIGGER = "Attack3";
		protected readonly string TAKE_DAMAGE_TRIGGER = "GetHit2";
		protected readonly string WALKING_BOOL = "Walking";

		#endregion

		[SerializeField] protected GolemStates lastState = GolemStates.Idle;
		[SerializeField] protected GolemStates actualState = GolemStates.Idle;

		//private bool isInChasingRange = false;
		public bool IsInAttackRange =>
			Vector3.Distance(player.transform.position, transform.position) < maxCloseDistance;

		protected virtual void Awake() {
			healthBarController = GetComponent<HealthBarController>();
			Health = maxLife;
		}

		protected virtual void Update() {
			#if DEBUG
			if(Input.GetKeyDown(KeyCode.M)) {
				CanMove = !CanMove;
			}
			#endif

			if (Health <= 0)
				isDead = true;

			if (player) {
				if (!isDead) {
					if (player.Health > 0) {
						if (IsInAttackRange && canAttack &&
						    (actualState == GolemStates.Idle || actualState == GolemStates.Walking) && !takingDamage) {
							if (canAttack) {
								if (Debug.isDebugBuild) Debug.Log("Attack");
								Attack();
							}
						} else if (player.Health > 0 && IsChasing /*&& (!isSpecialAttackTriggered || !canAttack)*/ &&
						           (actualState == GolemStates.Idle ||
						            actualState == GolemStates.Walking) /* && !takingDamage*/) {
							Walk();
						}
					} else //When the player is dead
					{
						ChangeState(GolemStates.Idle);
						SetAnimationBool(WALKING_BOOL, false);
					}
				} else {
					if (!animDeadPlayed) {
						Die();
					}
				}
			}
		}

		private void OnEnable() {
			if(healthBarHudItem) healthBarHudItem.SetActive(true);
			else Debug.LogError($"healthBarHudItem has not been assigned", this);
		}

		public void SetPlayer(PlayerController _player) {
			player = _player;
		}

		protected void ChangeState(GolemStates _newState) {
			lastState = actualState;
			actualState = _newState;
		}

		public abstract void TriggerSpecialAttack(); //Esto lo debe de desarrolar el golem hijo de esta clase

		/// <summary>
		/// Used to play animations with a animator trigger
		/// </summary>
		/// <param name="_triggerName"></param>
		protected void PlayAnimationWithTrigger(string _triggerName) {
			if (_triggerName == SPECIAL_ATTACK_TRIGGER && (takingDamage && !canAttack)) {
				isSpecialAttackTriggered = false;
				return;
			}

			animator.SetTrigger(_triggerName);
			if (_triggerName == SPECIAL_ATTACK_TRIGGER) {
				SetAnimationBool(WALKING_BOOL, false);
			} else if (_triggerName == ATTACK_TRIGGER) {
				SetAnimationBool(WALKING_BOOL, false);
			} else if (_triggerName == DEATH_TRIGGER) {
				SetAnimationBool(WALKING_BOOL, false);
			}
		}

		/// <summary>
		/// Used to play animations with a animator boolean
		/// </summary>
		/// <param name="_boolName"></param>
		/// <param name="_makingSomething"></param>
		protected void SetAnimationBool(string _boolName, bool _makingSomething) {
			animator.SetBool(_boolName, _makingSomething);
		}

		public void Walk() {
			if (!CanMove) return;
			if (animator.GetCurrentAnimatorStateInfo(0).IsName(WALK_STATE_NAME) ||
			    animator.GetCurrentAnimatorStateInfo(0).IsName(IDLE_STATE_NAME)) {
				Vector3 playerPos = new Vector3(player.transform.position.x, transform.position.y,
					player.transform.position.z);
				if (actualState != GolemStates.Walking) ChangeState(GolemStates.Walking);
				transform.LookAt(new Vector3(player.transform.position.x, transform.position.y,
					player.transform.position.z));
				transform.position = Vector3.MoveTowards(transform.position, playerPos, walkSpeed * Time.deltaTime);
				SetAnimationBool(WALKING_BOOL, true);
			}
		}

		public void TakeDamage(int _damage) {
			if (!takingDamage) {
				canAttack = false;
				takingDamage = true;
				StopAllCoroutines();
				StartCoroutine(TakeDamageWithCoroutine(_damage));
			}

		}

		protected IEnumerator TakeDamageWithCoroutine(int _damage) {
			if (!isDead) {
				shield.SetActive(true);
				ChangeState(GolemStates.TakingDamage);
				Health -= _damage;
				SetAnimationBool(WALKING_BOOL, false);
				PlayAnimationWithTrigger(TAKE_DAMAGE_TRIGGER);
				yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
				ChangeState(GolemStates.Idle);
			}

			yield return new WaitForSeconds(cooldownGetDamage);
			takingDamage = false;
			shield.SetActive(false);
			canAttack = true;
		}

		protected void Attack() {
			if (IsInoffensiveGolem) return;
			if (!takingDamage) {
				canAttack = false;
				ChangeState(GolemStates.Attacking);
				PlayAnimationWithTrigger(ATTACK_TRIGGER);
				StartCoroutine(CooldownAttack());
				StartCoroutine(ChangeStateWhenAnimationIsOver(NORMAL_ATTACK_STATE_NAME, GolemStates.Idle));
			}
		}

		protected IEnumerator ChangeStateWhenAnimationIsOver(string _actualAnimationName, GolemStates _stateToChange) {
			while (!animator.GetCurrentAnimatorStateInfo(0).IsName(_actualAnimationName)) {
				yield return null;
			}

			while (animator.GetCurrentAnimatorStateInfo(0).IsName(_actualAnimationName)) {
				yield return null;
			}

			ChangeState(_stateToChange);
		}

		protected IEnumerator CooldownAttack() {
			yield return new WaitForSeconds(attackCooldownTime);
			canAttack = true;
		}

		public bool IsInnoffensiveGolem() {
			return IsInoffensiveGolem;
		}

		protected void Die() {
			collision.enabled = false;
			PlayAnimationWithTrigger(DEATH_TRIGGER);
			winCanvas.SetActive(true);
			animDeadPlayed = true;
			GameManager.Instance.SetPlayerVictory();
		}
	}
}