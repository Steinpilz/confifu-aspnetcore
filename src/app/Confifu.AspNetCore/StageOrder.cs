namespace Confifu.AspNetCore
{
    class StageOrder
    {
        public string FirstStage { get; }

        public string NextStage { get; }

        public StageOrder(string firstStage, string nextStage)
        {
            this.FirstStage = firstStage;
            this.NextStage = nextStage;
        }
    }
}