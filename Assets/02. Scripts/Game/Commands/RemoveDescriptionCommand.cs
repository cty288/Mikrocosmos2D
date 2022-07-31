using MikroFramework.Architecture;

namespace Mikrocosmos {

    public struct OnDescriptionRemoved {
        public DescriptionType Type;
    }
    public class RemoveDescriptionCommand : AbstractCommand<RemoveDescriptionCommand> {
        private DescriptionType descriptionType;

        public RemoveDescriptionCommand(DescriptionType descriptionType)
        {
            this.descriptionType = descriptionType;
        }

        public RemoveDescriptionCommand() {
            
        }

        protected override void OnExecute() {
            this.SendEvent<OnDescriptionRemoved>(new OnDescriptionRemoved() {Type = descriptionType});
        }
    }
}