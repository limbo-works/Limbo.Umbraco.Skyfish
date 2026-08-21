// [CHANGE: Umbraco 17 upgrade - IEditorConfigurationParser was removed from the ConfigurationEditor constructor] Related: PropertyEditors/SkyfishVideoPropertyEditor.cs, PropertyEditors/SkyfishVideoConfiguration.cs

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;

#pragma warning disable CS1591

namespace Limbo.Umbraco.Skyfish.PropertyEditors;

public class SkyfishVideoConfigurationEditor : ConfigurationEditor<SkyfishVideoConfiguration> {

    public SkyfishVideoConfigurationEditor(IIOHelper ioHelper) : base(ioHelper) { }

}
