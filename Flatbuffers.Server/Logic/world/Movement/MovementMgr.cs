using System.Reflection;
using Game.Logic.datatable;
using log4net;
using Logic.database;
using Logic.database.table;

namespace Game.Logic.World.Movement;

public class MovementMgr
{
	private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	private static Dictionary<string, DBPath> m_pathCache = new Dictionary<string, DBPath>();
	private static Dictionary<string, SortedList<int, DBPathPoint>> m_pathpointCache = new Dictionary<string, SortedList<int, DBPathPoint>>();
	private static object LockObject = new object();

	private static void FillPathCache()
	{
		IList<DBPath> allPaths = GameServer.Database.SelectAllObjects<DBPath>();
		foreach (DBPath path in allPaths)
		{
			m_pathCache.Add(path.PathID, path);
		}

		int duplicateCount = 0;

		IList<DBPathPoint> allPathPoints = GameServer.Database.SelectAllObjects<DBPathPoint>();
		foreach (DBPathPoint pathPoint in allPathPoints)
		{
			if (m_pathpointCache.ContainsKey(pathPoint.PathID))
			{
				if (m_pathpointCache[pathPoint.PathID].ContainsKey(pathPoint.Step) == false)
				{
					m_pathpointCache[pathPoint.PathID].Add(pathPoint.Step, pathPoint);
				}
				else
				{
					duplicateCount++;
				}
			}
			else
			{
				SortedList<int, DBPathPoint> pList = new SortedList<int, DBPathPoint>();
				pList.Add(pathPoint.Step, pathPoint);
				m_pathpointCache.Add(pathPoint.PathID, pList);
			}
		}
        if (duplicateCount > 0)
            log.ErrorFormat("{0} duplicate steps ignored while loading paths.", duplicateCount);
		log.InfoFormat("Path cache filled with {0} paths.", m_pathCache.Count);
	}

	public static void UpdatePathInCache(string pathID)
	{
		log.DebugFormat("Updating path {0} in path cache.", pathID);

		var dbpath = GameDB<DBPath>.SelectObject(DB.Column(nameof(DBPath.PathID)).IsEqualTo(pathID));
		if (dbpath != null)
		{
			if (m_pathCache.ContainsKey(pathID))
			{
				m_pathCache[pathID] = dbpath;
			}
			else
			{
				m_pathCache.Add(dbpath.PathID, dbpath);
			}
		}

		var pathPoints = GameDB<DBPathPoint>.SelectObjects(DB.Column(nameof(DBPathPoint.PathID)).IsEqualTo(pathID));
		SortedList<int, DBPathPoint> pList = new SortedList<int, DBPathPoint>();
		if (m_pathpointCache.ContainsKey(pathID))
		{
			m_pathpointCache[pathID] = pList;
		}
		else
		{
			m_pathpointCache.Add(pathID, pList);
		}

		foreach (DBPathPoint pathPoint in pathPoints)
		{
			m_pathpointCache[pathPoint.PathID].Add(pathPoint.Step, pathPoint);
		}
	}

    public static PathPoint LoadPath(string pathID)
    {
        lock(LockObject)
        {
	        if (m_pathCache.Count == 0)
			{
				FillPathCache();
			}

			DBPath dbpath = null;

			if (m_pathCache.ContainsKey(pathID))
			{
				dbpath = m_pathCache[pathID];
			}

            ePathType pathType = ePathType.Once;

            if (dbpath != null)
            {
                pathType = (ePathType)dbpath.PathType;
            }

			SortedList<int, DBPathPoint> pathPoints = null;

			if (m_pathpointCache.ContainsKey(pathID))
			{
				pathPoints = m_pathpointCache[pathID];
			}
			else
			{
				pathPoints = new SortedList<int, DBPathPoint>();
			}

            PathPoint prev = null;
            PathPoint first = null;

			foreach (DBPathPoint dbPathPoint in pathPoints.Values)
			{
				var pathPoint = new PathPoint(dbPathPoint, pathType);

				if (first == null)
				{
					first = pathPoint;
				}
				pathPoint.Prev = prev;
				if (prev != null)
				{
					prev.Next = pathPoint;
				}
				prev = pathPoint;
			}

            return first;
        }
    }

    public static void SavePath(string pathID, PathPoint path)
    {
        if (path == null)
            return;

        pathID.Replace('\'', '/');
		var dbpath = GameDB<DBPath>.SelectObject(DB.Column(nameof(DBPath.PathID)).IsEqualTo(pathID));
		if (dbpath != null)
		{
			GameServer.Database.DeleteObject(dbpath);
		}

		GameServer.Database.DeleteObject(GameDB<DBPathPoint>.SelectObjects(DB.Column(nameof(DBPathPoint.PathID)).IsEqualTo(pathID)));
        PathPoint root = FindFirstPathPoint(path);
        path = root;
        dbpath = new DBPath(pathID, root.Type);
        GameServer.Database.AddObject(dbpath);

        int i = 1;
        do
        {
            var dbPathPoint = path.GenerateDbEntry();
            dbPathPoint.Step = i++;
            dbPathPoint.PathID = pathID;
            GameServer.Database.AddObject(dbPathPoint);
            path = path.Next;
        }
		while (path != null && path != root);

		UpdatePathInCache(pathID);
    }

    public static PathPoint FindFirstPathPoint(PathPoint path)
    {
        PathPoint root = path;
        // avoid circularity
        int iteration = 50000;
        while (path.Prev != null && path.Prev != root)
        {
            path = path.Prev;
            iteration--;
            if (iteration <= 0)
            {
                if (log.IsErrorEnabled)
                    log.Error("Path cannot be saved, it seems endless");
                return null;
            }
        }
        return path;
    }
}