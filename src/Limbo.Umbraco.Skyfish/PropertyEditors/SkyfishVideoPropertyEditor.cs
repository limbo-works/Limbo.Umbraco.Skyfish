using Limbo.Umbraco.Skyfish.Constants;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;

#pragma warning disable CS1591

namespace Limbo.Umbraco.Skyfish.PropertyEditors;

/// <summary>
/// Represents the property editor schema for the Skyfish video picker.
/// </summary>
/// <remarks>
/// The name, icon, group and the matching property editor UI are all registered client side from <c>EntryPoint.js</c> - see <see cref="EditorUiAlias"/>.
/// </remarks>
[DataEditor(EditorAlias, ValueType = EditorValueType)]
public class SkyfishVideoPropertyEditor : DataEditor {

    private readonly IIOHelper _ioHelper;

    #region Constants

    public const string EditorAlias = SkyfishPropertyEditorSchemaAliases.Video;

    public const string EditorUiAlias = SkyfishPropertyEditorUiAliases.Video;

    public const string EditorName = "Limbo Skyfish Video";

    public const string EditorIcon = "limbo-skyfish-alt";

    public const string EditorGroup = "Limbo";

    public const string EditorValueType = ValueTypes.Json;

    #endregion

    #region Constructors

    public SkyfishVideoPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper) : base(dataValueEditorFactory) {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    #endregion

    #region Member methods

    protected override IConfigurationEditor CreateConfigurationEditor() {
        return new SkyfishVideoConfigurationEditor(_ioHelper);
    }

    #endregion

}