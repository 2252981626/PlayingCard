using Newtonsoft.Json;

public class ApiResponse<T>
{
    public int code { get; set; }
    public string message { get; set; }
    public T data { get; set; }
}

/// <summary>
/// ‘∂≥Ã≈‰÷√
/// </summary>
public class ResponseRemoteConfig
{
    public float ad_start_time { get; set; }
    public float ad_inter_time { get; set; }
    public float ad_mau_inter_time { get; set; }
}

/// <summary>
/// App Att
/// </summary>
public class ResponseAppATTConfig
{
    public string att_code { get; set; }
}

public class ResponseDeviceRegisterConfig
{
    public long timestamp { get; set; }
    public object is_vip { get; set; }
    public ResponseIp_info ip_Info { get; set; }
}

public class ResponseIp_info
{
    public string ip;
    public string country_name;
    public string country_code;
    public string region_name;
    public string city_name;
    public string latitude;
    public string longitude;
}