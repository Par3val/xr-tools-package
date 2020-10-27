using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[HideInInspector]
	public Rigidbody rb;
	public PlayerRig rig;
	public CapsuleCollider col;

	const float headSize = 0.1524f;

	private void Awake()
	{
		rig = GetComponentInParent<PlayerRig>();
		//var playerRb = rig.gameObject.AddComponent<Rigidbody>();
		//playerRb = GetComponent<Rigidbody>();
		//var playerCol = rig.gameObject.AddComponent<CapsuleCollider>();
		//playerCol = GetComponent<CapsuleCollider>();

		//Destroy(GetComponent<Rigidbody>());
		//Destroy(GetComponent<CapsuleCollider>());
	}

	void FixedUpdate()
	{
		if (rb && col)
		{
			col.center = AjustedHeadPos();
			col.height = rig.alias.HeadsetAlias.transform.localPosition.y + headSize;
		}
	}

	public void SetClimb(bool isClimbing)
	{
		rb.useGravity = !isClimbing;
		col.isTrigger = isClimbing;
	}

	Vector3 AjustedHeadPos()
	{
		Vector3 headPos = rig.alias.HeadsetAlias.transform.localPosition;
		return new Vector3(headPos.x, (headPos.y + headSize) / 2, headPos.z);
	}

	private void OnCollisionEnter(Collision collision)
	{

	}

	private void OnCollisionExit(Collision collision)
	{

	}
}
