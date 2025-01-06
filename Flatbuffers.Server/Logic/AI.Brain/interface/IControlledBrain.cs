namespace Game.Logic.AI.Brain
{
    public enum eWalkState
    {
        /// <summary>
        /// Follow the owner
        /// </summary>
        Follow,
        /// <summary>
        /// Don't move if not in combat
        /// </summary>
        Stay,
        ComeHere,
        GoTarget,
    }

    public enum eAggressionState
    {
        /// <summary>
        /// Attack any enemy in range
        /// </summary>
        Aggressive,
        /// <summary>
        /// Attack anything that attacks brain owner or owner of brain owner
        /// </summary>
        Defensive,
        /// <summary>
        /// Attack only on order
        /// </summary>
        Passive,
    }
    
    public interface IControlledBrain
    {
        eWalkState WalkState { get; }
        eAggressionState AggressionState { get; set; }
        GameNPC Body { get; }
        GameLiving Owner { get; }
        void Attack(GameObject target);
        void Follow(GameObject target);
        void FollowOwner();
        void Stay();
        void ComeHere();
        void Goto(GameObject target);
        void UpdatePetWindow();
        GamePlayer GetPlayerOwner();
        GameNPC GetNPCOwner();
        GameLiving GetLivingOwner();
        void SetAggressionState(eAggressionState state);
        bool IsMainPet { get; set; }
    }
}