namespace NetBackend.Types;

public class AccessKeyPermissionInputType : InputObjectType<AccessKeyPermissionInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<AccessKeyPermissionInput> descriptor)
    {
        descriptor.Name("AccessKeyPermissionInput");
        descriptor.Field(t => t.QueryName).Type<NonNullType<StringType>>();
        descriptor.Field(t => t.AllowedFields).Type<ListType<StringType>>();
    }
}

public class AccessKeyPermissionInput
{
    public required string QueryName { get; set; }
    public required List<string> AllowedFields { get; set; }
}