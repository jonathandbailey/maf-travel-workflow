namespace Tests.Common;

public static class Data
{
    public const string ActAgentDepartureCityResponse = """
        What is your departure city, and what date do you plan to depart? 
        ```json
        {
          "route": "ask_user",
          "metadata": {
            "reason": "Requesting missing input for flight research"
          }
        }
        ```
        """;

    public const string ActAgentUserComplete = """
        All required information now complete. 
        ```json
        {
          "route": "workflow_complete",
          "metadata": {
            "reason": "Requesting missing input for flight research"
          }
        }
        ```
        """;

    public const string ReasonTripToParisDeparturePointRequired =
        "User want to plan a trip to Paris.Departure Point is required";

    public const string ReasonTripInformationCompete =
        "User has now provided all the required information.";

    public const string PlanTripToParisUserRequest = "I want to plan a trip to Paris";

    public const string ActAgentDepartureCityUserResponse = "What is your departure city, and what date do you plan to depart?";

    public const string UserDepartingFromCity = "I'm departing from Dublin";
}
