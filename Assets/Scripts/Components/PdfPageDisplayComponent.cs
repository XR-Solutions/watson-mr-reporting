using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Components
{
	[RequireComponent(typeof(RawImage))]
	[RequireComponent(typeof(RectTransform))]
	public class PdfPageDisplay : MonoBehaviour
	{
		public RawImage pdfPageImage;

		public void Start()
		{
			pdfPageImage = GetComponent<RawImage>();

			if (pdfPageImage == null)
			{
				Debug.LogError("No rawimage found in the scene!");
			}
		}

		public void SetPdfPageImage(Texture2D texture)
		{
			pdfPageImage.texture = texture;
		}
	}
}
