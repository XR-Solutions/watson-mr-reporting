using Assets.Scripts.Models;

public class Note
{
	public string Name { get; private set; }
	public string Description { get; private set; }
	public TraceTypes TraceType { get; private set; }

	public Note(string name, string description, TraceTypes traceType)
	{
		Name = name;
		Description = description;
		TraceType = traceType;
	}
}
