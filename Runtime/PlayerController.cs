using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public PlayerRig rig;
	[HideInInspector]
	public Rigidbody rb;
	[HideInInspector]
	public CapsuleCollider col;

	public bool useGravity = true;
	public bool usePlayerHeight = true;
	public bool affectTrigger = true;

	public float jumpPower = 2.5f;
	const float headSize = 0.1524f;

	private void Awake()
	{
		if (!rig)
			rig = GetComponentInParent<PlayerRig>();

		if (!rb)
			rb = rig.GetComponent<Rigidbody>();
		if (!col)
			col = rig.GetComponent<CapsuleCollider>();
	}

	void FixedUpdate()
	{
		if (rb && col)
		{
			col.center = usePlayerHeight ? AjustedHeadPos() : rig.alias.HeadsetAlias.transform.localPosition;
			col.height = (usePlayerHeight ? rig.alias.HeadsetAlias.transform.localPosition.y : 0) + headSize;
		}
	}

	public void SetClimb(bool isClimbing)
	{
		rb.useGravity = !isClimbing && useGravity;
		if (affectTrigger)
			col.isTrigger = isClimbing;
		rb.velocity *= isClimbing ? 0 : 1;
	}

	Vector3 AjustedHeadPos()
	{
		Vector3 headPos = rig.alias.HeadsetAlias.transform.localPosition;
		return new Vector3(headPos.x, (headPos.y + headSize) / 2, headPos.z);
	}

	public void Jump()
	{
		Debug.Log("jump");
		rb.velocity += transform.up * jumpPower;
	}

	private void OnCollisionEnter(Collision collision)
	{

	}

	private void OnCollisionExit(Collision collision)
	{

	}
}
