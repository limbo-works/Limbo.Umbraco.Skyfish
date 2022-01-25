using Skybrud.VideoPicker.Providers;
using Umbraco.Core.Composing;

namespace Skybrud.VideoPicker.Skyfish.Composers {

    internal class SkyfishComposer : IUserComposer {

        public void Compose(Composition composition) {
            composition.VideoPickerProviders().Append<SkyfishVideoProvider>();
        }

    }

}