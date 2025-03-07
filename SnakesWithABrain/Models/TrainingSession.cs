namespace SnakesWithABrain
{
    public class TrainingSession
    {
        public int arrayX = 0;
        public int arrayY = 0;
        public string GUID = "";
        public bool canWrap = false;
        public int randomDeathChance = 0;//% chance for a GOOD LSTM to just vanish
        public int genSize = 20;
        public int keepCount = 6;
        public int breedCount = 4;
        public int mutateCount = 4;
        public int mutateChance = 20;
        public float foodEatScaling = 1.5f;
        public float crashScaling = 1.2f;
        public float eatSelfScaling = 1.2f;
        public float starveScaling = 1f;
        public int inputType = 1;
        public int startingLength = 0;
        public int maxSegmentLength = 999;
        //1 = Surrounding blocked area and segment count
        //2 = Surrounding Coords and are they blocked
        //3 = food, self, whole board

        //gets updated during runtime.
        public int generation = 1;
        public int snakeIndex = 0;
        public int foodX = 0;
        public int foodY = 0;
        public double bestFitnessEver = 0;
        public double mostFoodEver = 0;
        public bool newFoodNeeded = true;//init true to spawn new food on start.
        public int inputCount = 1;
        public int outputCount = 8;
    }
}
