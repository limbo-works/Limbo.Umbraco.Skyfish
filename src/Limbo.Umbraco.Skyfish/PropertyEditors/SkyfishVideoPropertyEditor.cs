// [CHANGE: Umbraco 17 upgrade - the editor no longer points at a server side view, the UI is registered client side instead] Related: wwwroot/EntryPoint.js, PropertyEditors/SkyfishVideoConfigurationEditor.cs, PropertyEditors/SkyfishVideoConfiguration.cs

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;

#pragma warning disable CS1591

namespace Limbo.Umbraco.Skyfish.PropertyEditors;

/// <summary>
/// Represents the property editor schema for the Skyfish video picker.
/// </summary>
/// <remarks>
/// The name, icon, group and the matching property editor UI are all registered client side from
/// <c>EntryPoint.js</c> - see <see cref="EditorUiAlias"/>.
/// </remarks>
[DataEditor(EditorAlias, ValueType = ValueTypes.Json)]
public class SkyfishVideoPropertyEditor : DataEditor {

    private readonly IIOHelper _ioHelper;

    #region Constants

    /// <summary>
    /// Gets the alias of the property editor schema. This is the alias stored in the database for data types using
    /// this property editor, so it must not be changed.
    /// </summary>
    public const string EditorAlias = "Limbo.Umbraco.Skyfish.Video";

    /// <summary>
    /// Gets the alias of the property editor UI registered from <c>EntryPoint.js</c>.
    /// </summary>
    public const string EditorUiAlias = "Limbo.Umbraco.Skyfish.Video.Ui";

    public const string EditorName = "Limbo Skyfish Video";

    public const string EditorIcon = "limbo-skyfish-alt";

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