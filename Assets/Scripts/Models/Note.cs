using Assets.Scripts.Models;

[System.Serializable]
public class Note
{
	public string Guid;
	public string Name;
	public string Description;
	public TraceTypes TraceType;
	public ObjectMetadata ObjectMetadata;


	public Note(string guid, string name, string description, TraceTypes traceType, ObjectMetadata objectMetadata)
	{
		Guid = guid;
		Name = name;
		Description = description;
		TraceType = traceType;
		ObjectMetadata = objectMetadata;
	}
}