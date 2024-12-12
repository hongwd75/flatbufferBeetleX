using System.Collections.Concurrent;
using System.Numerics;

namespace Game.Logic.World;
/*
 *  Rgion
 *   - zone
 *   - zone
 */ 
public class Region
{
    protected int mNextObjectSlot = 0; // 최대 index 값
    protected Queue<int> mFreeObjectSlot = new Queue<int>(); // 사용하지 않는 index값
    protected Dictionary<int, GameObject> mObjects = new();   // 사용하고 있는 index값
    
    public readonly object ObjectsSyncLock = new object();    // 락용 변수   


    //----------------------------------------------------------------------------------------------------------------
    #region 추가 / 삭제 함수
    internal bool AddObject(GameObject obj)
    {
        lock (ObjectsSyncLock)
        {
            int index = -1;
            if (obj.ObjectID != -1)
            {
                Console.WriteLine($"[오류] {obj.Name} / {obj.GetType().FullName}객체 인덱스가 이미 정의되어 있음. 확인이 필요함.");
                return false;
            }
            
            if (mFreeObjectSlot.Count > 0)
            {
                index = mFreeObjectSlot.Dequeue();
            }
            else
            {
                index = mNextObjectSlot;
                mNextObjectSlot++;
            }

            obj.ObjectID = index;
            mObjects.Add(index,obj);
            return true;
        }
    }

    internal bool RemoveObject(GameObject obj)
    {
        lock (ObjectsSyncLock)
        {
            if (obj == null || obj.ObjectID < 0)
            {
                return false;
            }

            mObjects.Remove(obj.ObjectID);
            mFreeObjectSlot.Enqueue(obj.ObjectID);
            obj.ObjectID = -1;
            return true;
        }
    }
    #endregion
}