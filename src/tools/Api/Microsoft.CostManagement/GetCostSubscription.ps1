# Authenticate and select the subscription and tenant
$tenantId = "86759412-e019-4f10-a127-be41d0dcbc72"
$subscriptionId = "e595abb5-209a-4cce-8515-6311102caecb"

Connect-AzAccount -TenantId $tenantId -SubscriptionId $subscriptionId

# Define the variables
$startDate = "2024-06-01"
$endDate = "2024-06-30"
$apiVersion = "2023-11-01"

# Construct the URI to fetch the aggregated cost
$baseUri = "https://management.azure.com/subscriptions/$subscriptionId/providers/Microsoft.CostManagement/query?api-version=$apiVersion"

# Define the query to get the total cost for the month
$query = @{
    type = "Usage"
    timeframe = "Custom"
    timePeriod = @{
        from = $startDate
        to = $endDate
    }
    dataset = @{
        granularity = "None"
        aggregation = @{
            totalCost = @{
                name = "Cost"
                function = "Sum"
            }
        }
    }
} | ConvertTo-Json -Depth 3

# Get the cost data
try {
    $token = (Get-AzAccessToken -ResourceUrl https://management.azure.com).Token
    $response = Invoke-RestMethod -Uri $baseUri -Method Post -Headers @{
        Authorization = "Bearer $token"
        "Content-Type" = "application/json"
    } -Body $query -ErrorAction Stop

    # Extract the total cost for the month
    if ($response.properties.rows.Count -gt 0) {
        $totalCost = $response.properties.rows[0][0]  # Adjust index based on your response structure
        Write-Output "Total Cost: $totalCost"
    } else {
        Write-Output "No cost data found for June 2024."
    }
} catch {
    Write-Error "Error occurred while fetching cost data: $_"
}