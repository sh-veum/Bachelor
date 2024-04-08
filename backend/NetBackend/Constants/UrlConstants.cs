namespace NetBackend.Constants;

public static class UrlConstants
{
    // TODO: need to be changed once the front-end is containerized
    public const string FrontEndURL = "http://localhost:5173";

    // For the docker network
    public const string MockSensorURL = "http://mock-sensors:80";
}