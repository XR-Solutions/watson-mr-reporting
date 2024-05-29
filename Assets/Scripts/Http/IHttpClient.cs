using System.Collections;

namespace Assets.Scripts.Http
{
	internal interface IHttpClient<in T> where T : class
	{
		public IEnumerable Post(T Object);
	}
}
