using UnityEngine;

public abstract class Singleton<T> : Singleton where T : MonoBehaviour
{
	private static T instance;
	private static readonly object Lock = new object();

	[SerializeField]
	private bool persistant = true;

	public static T Instance
	{
		get
		{
			if (Singleton.Quitting)
			{
				return null;
			}
			lock (Lock)
			{
				if ((Object)instance != (Object)null)
				{
					return instance;
				}
				T[] array = Object.FindObjectsOfType<T>();
				int num = array.Length;
				if (num > 0)
				{
					if (num == 1)
					{
						return instance = array[0];
					}
					for (int i = 1; i < array.Length; i++)
					{
						Object.Destroy(array[i]);
					}
					return instance = array[0];
				}
				return null;
			}
		}
	}

	private void Awake()
	{
		if (persistant)
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
		OnAwake();
	}

	protected virtual void OnAwake()
	{
	}
}
public abstract class Singleton : MonoBehaviour
{
	public static bool Quitting { get; private set; }

	private void OnApplicationQuit()
	{
		Quitting = true;
	}
}
