using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Redirects collisions as a message to the target GameObject.
/// Only handles enter/exit for performance reasons.
/// 
/// @author rhagan
/// </summary>
public class ForwardCollisions : MonoBehaviour {

	/// <summary>
	/// The target to redirect collision messages to.
	/// </summary>
	public GameObject target;

	public string message;

	/// <summary>
	/// Called on starting collision.
	/// </summary>
	/// <param name='collision'>
	/// Collision.
	/// </param>
	private void OnCollisionEnter(Collision other) {
		target.SendMessage(string.IsNullOrEmpty(message) ?
		                   "OnCollisionEnter" : message, other, SendMessageOptions.DontRequireReceiver);
	}

	/// <summary>
	/// Raises the collision exit event.
	/// </summary>
	/// <param name="other">Other.</param>
	private void OnCollisionExit(Collision other) {
		target.SendMessage("OnCollisionExit", other, SendMessageOptions.DontRequireReceiver);
	}
}
