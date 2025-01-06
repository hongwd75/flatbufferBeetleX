namespace Game.Logic.Utils
{
    public class RandomUtil
    {
        [ThreadStatic]
        private Random mRandom = null;
        private static RandomUtil instance = new RandomUtil();

        #region ======================= 랜덤 함수 ==========================
        public static bool Bool() => instance.RandomImpl(0, 1) == 0;
        public static int Int(int max) => instance.RandomImpl(0, max);
        public static int Int(int min,int max) => instance.RandomImpl(min, max);
        public static double Double() => instance.RandomDoubleImpl();
        public static bool Chance(int percent) => percent >= instance.RandomImpl(0, 100);
        public static bool Chance(double percent) => percent > instance.RandomDoubleImpl();
        #endregion

        #region =====================  랜덤 내부 로직  =======================
        protected Random RandomGen
        {
            get
            {
                if (mRandom == null)
                {
                    mRandom = new Random();
                }
                return instance.mRandom;
            }
        }
        
        protected double RandomDoubleImpl()
        {
            return RandomGen.NextDouble();
        }
        protected int RandomImpl(int min, int max)
        {
            return RandomGen.Next(min, max + 1);
        }
        #endregion
    }
}