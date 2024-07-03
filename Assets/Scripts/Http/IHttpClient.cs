using System.Collections;

namespace Assets.Scripts.Http
{
	internal interface IHttpClient<T> where T : class
	{
		public IEnumerator Post(T Object);
		public T GetAll();
	}
}
