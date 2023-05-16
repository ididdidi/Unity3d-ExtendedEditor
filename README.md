# Unity3d-ExtendedEditor
This is my toolkit for extending the Unity3d editor

- **[ExtendedEditor](#extendededitor)**
- **[Attributes](#attributes)**

## ExtendedEditor
Extending the capabilities of the standard Unity3d Editor. The table lists methods that extend the base class Editor.

Method									| Description
--------------------------------------- | -----------
[AddPropertyView](#addpropertyview)		| Adding displayable serialized object properties
[DisplayMessage](#displaymessage)		| Display message in Unity3d inspector
[OnInspectorGUI](#oninspectorgui)		| Create a Custom Inspector
[DrawProperties](#drawproperties)		| Display serialized object properties

### AddPropertyView
Method takes a `SerializedProperty`, label of the field and displayable serialized object properties. It is best to use it in `OnEnable()` method 
```csharp
private void OnEnable()
{
    AddPropertyView(serializedObject.FindProperty("propertyName"));
}
```

### DisplayMessage
Static method takes a string, a message type as parameters and to display message in Unity3d inspector.

### OnInspectorGUI
Overrides the base method for creating a custom inspector.

### DrawProperties
Displaying the serialized properties of an object added in the Add Property View() method.

## Attributes
Additional attributes module for Unity3d.

Attribute								| Description
--------------------------------------- | -----------
[Label](#labelattribute)				| Allows you to change the attribute name
[RequireType](#requiretype)				| Allows the field to be displayed for the provided interface in the inspector

### LabelAttribute
Add the following attribute before the property declaration to change its label displayed in the Inspector:
```csharp
[Label("Property name")] string str;
```

### RequireType
Add the following attribute before the property declaration of type `Object`(!) to display the field for the object in the Inspector:
```csharp
[RequireType(typeof(InterfaceType))] Object fieldName;
```
`InterfaceType` - is the name of the interface (required parameter).