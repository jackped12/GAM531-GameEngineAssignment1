using OpenTK.Mathematics;

namespace Windows_Engine
{
    public class Interact
    {
        public Vector3 ItemPosition;
        private bool collected;

        public Interact(Vector3 position)
        {
            ItemPosition = position;
            collected = false;
        }

        public bool TryCollect(Vector3 playerPos, float radius = 1.5f)
        {
            if (collected)
                return false;

            if ((playerPos - ItemPosition).Length < radius)
            {
                collected = true;
                return true;
            }
            return false;
        }

        public bool IsCollected => collected;
    }
}
