namespace Assets.Scripts.Models
{
	[System.Serializable]
	public class ObjectMetadata
	{
		public float[] Position;
		public float[] Rotation;
		public float[] Scale;
		public bool Enabled;

		public ObjectMetadata(float[] position, float[] rotation, float[] scale, bool enabled)
		{
			Position = position;
			Rotation = rotation;
			Scale = scale;
			Enabled = enabled;
		}
	}
}
