using System;
using Limbo.Umbraco.Skyfish.Models.Videos;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Newtonsoft;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

#pragma warning disable 1591

namespace Limbo.Umbraco.Skyfish.PropertyEditors;

/// <summary>
/// Property value converter for <see cref="SkyfishEditor"/>.
/// </summary>
public class SkyfishValueConverter : PropertyValueConverterBase {

    public override bool IsConverter(IPublishedPropertyType propertyType) {
        return propertyType.EditorAlias == SkyfishEditor.EditorAlias;
    }

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview) {
        return source is string str && str.DetectIsJson() ? JsonUtils.ParseJsonObject(str) : null;
    }

    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview) {
        return inter is JObject json ? SkyfishVideoValue.Create(json, propertyType.DataType.Configuration as SkyfishConfiguration) : null;
    }

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType) {
        return typeof(SkyfishVideoValue);
    }

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) {
        return PropertyCacheLevel.Element;
    }

}