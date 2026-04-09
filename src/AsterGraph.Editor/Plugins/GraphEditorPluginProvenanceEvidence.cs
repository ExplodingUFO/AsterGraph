namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 表示插件包标识。
/// </summary>
public sealed record GraphEditorPluginPackageIdentity
{
    /// <summary>
    /// 初始化包标识。
    /// </summary>
    public GraphEditorPluginPackageIdentity(string id, string? version = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Id = id.Trim();
        Version = string.IsNullOrWhiteSpace(version) ? null : version.Trim();
    }

    /// <summary>
    /// 包 ID。
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// 可选包版本。
    /// </summary>
    public string? Version { get; }
}

/// <summary>
/// 表示签名者身份。
/// </summary>
public sealed record GraphEditorPluginSignerIdentity
{
    /// <summary>
    /// 初始化签名者身份。
    /// </summary>
    public GraphEditorPluginSignerIdentity(string displayName, string? fingerprint = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        DisplayName = displayName.Trim();
        Fingerprint = string.IsNullOrWhiteSpace(fingerprint) ? null : fingerprint.Trim();
    }

    /// <summary>
    /// 宿主可读签名者名称。
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// 可选的稳定指纹标识。
    /// </summary>
    public string? Fingerprint { get; }
}

/// <summary>
/// 指示插件签名的类型。
/// </summary>
public enum GraphEditorPluginSignatureKind
{
    /// <summary>
    /// 未知或未声明。
    /// </summary>
    Unknown,

    /// <summary>
    /// 作者签名。
    /// </summary>
    Author,

    /// <summary>
    /// 仓库签名。
    /// </summary>
    Repository,
}

/// <summary>
/// 指示插件签名的当前状态。
/// </summary>
public enum GraphEditorPluginSignatureStatus
{
    /// <summary>
    /// 当前路径未提供签名证据。
    /// </summary>
    NotProvided,

    /// <summary>
    /// 有签名线索，但状态未知。
    /// </summary>
    Unknown,

    /// <summary>
    /// 明确未签名。
    /// </summary>
    Unsigned,

    /// <summary>
    /// 签名有效。
    /// </summary>
    Valid,

    /// <summary>
    /// 签名无效。
    /// </summary>
    Invalid,
}

/// <summary>
/// 表示一次稳定的签名证据快照。
/// </summary>
public sealed record GraphEditorPluginSignatureEvidence
{
    /// <summary>
    /// 初始化签名证据。
    /// </summary>
    public GraphEditorPluginSignatureEvidence(
        GraphEditorPluginSignatureStatus status,
        GraphEditorPluginSignatureKind kind = GraphEditorPluginSignatureKind.Unknown,
        GraphEditorPluginSignerIdentity? signer = null,
        DateTimeOffset? timestampUtc = null,
        string? timestampAuthority = null,
        string? reasonCode = null,
        string? reasonMessage = null)
    {
        Status = status;
        Kind = kind;
        Signer = signer;
        TimestampUtc = timestampUtc;
        TimestampAuthority = string.IsNullOrWhiteSpace(timestampAuthority) ? null : timestampAuthority.Trim();
        ReasonCode = string.IsNullOrWhiteSpace(reasonCode) ? null : reasonCode.Trim();
        ReasonMessage = string.IsNullOrWhiteSpace(reasonMessage) ? null : reasonMessage.Trim();
    }

    /// <summary>
    /// 当前签名状态。
    /// </summary>
    public GraphEditorPluginSignatureStatus Status { get; }

    /// <summary>
    /// 当前签名类型。
    /// </summary>
    public GraphEditorPluginSignatureKind Kind { get; }

    /// <summary>
    /// 可选签名者身份。
    /// </summary>
    public GraphEditorPluginSignerIdentity? Signer { get; }

    /// <summary>
    /// 可选时间戳时间。
    /// </summary>
    public DateTimeOffset? TimestampUtc { get; }

    /// <summary>
    /// 可选时间戳颁发方摘要。
    /// </summary>
    public string? TimestampAuthority { get; }

    /// <summary>
    /// 可选稳定原因代码。
    /// </summary>
    public string? ReasonCode { get; }

    /// <summary>
    /// 可选宿主可读原因文本。
    /// </summary>
    public string? ReasonMessage { get; }

    /// <summary>
    /// 当前路径未提供签名证据时的默认值。
    /// </summary>
    public static GraphEditorPluginSignatureEvidence NotProvided { get; } = new(GraphEditorPluginSignatureStatus.NotProvided);
}

/// <summary>
/// 表示一次稳定的来源和签名证据快照。
/// </summary>
public sealed record GraphEditorPluginProvenanceEvidence
{
    /// <summary>
    /// 初始化来源证据。
    /// </summary>
    public GraphEditorPluginProvenanceEvidence(
        GraphEditorPluginPackageIdentity? packageIdentity = null,
        GraphEditorPluginSignatureEvidence? signature = null)
    {
        PackageIdentity = packageIdentity;
        Signature = signature ?? GraphEditorPluginSignatureEvidence.NotProvided;
    }

    /// <summary>
    /// 可选包标识。
    /// </summary>
    public GraphEditorPluginPackageIdentity? PackageIdentity { get; }

    /// <summary>
    /// 当前签名证据。
    /// </summary>
    public GraphEditorPluginSignatureEvidence Signature { get; }

    /// <summary>
    /// 未提供额外来源证据时的默认值。
    /// </summary>
    public static GraphEditorPluginProvenanceEvidence NotProvided { get; } = new();
}
