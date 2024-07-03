using UnityEngine;

public class TextMeshProScaler : MonoBehaviour
{
	public Transform parentTransform;
	private Vector3 initialScale;

	void Start()
	{
		initialScale = transform.localScale;
	}

	void LateUpdate()
	{
		if (parentTransform != null)
		{
			// Maintain the original scale of the TextMeshPro object
			transform.localScale = new Vector3(
				initialScale.x / parentTransform.localScale.x / 3f,
				initialScale.y / parentTransform.localScale.y / 3f,
				initialScale.z / parentTransform.localScale.z / 3f
			);
		}
	}
}