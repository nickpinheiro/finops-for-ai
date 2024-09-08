###############################################################################################################################
# Azure OpenAI Resource Creation Deny Policy Script                                                                            #
#                                                                                                                              #
# This script creates a policy in Azure to deny the creation of Azure OpenAI resources across subscriptions or management      #
# groups.                                                                                                                      #
#                                                                                                                              #
# Features:                                                                                                                    #
# - Denies the creation of Azure OpenAI resources (Cognitive Services accounts).                                               #
# - Can be applied at subscription or management group level for organization-wide enforcement.                                #
# - Configures the policy to apply to all resource types (`Mode: All`).                                                        #
#                                                                                                                              #
# Requirements:                                                                                                                #
# - Azure PowerShell module for authentication and policy creation.                                                            #
# - Input parameters: subscription or management group ID.                                                                     #
###############################################################################################################################

# Authenticate and select the subscription and tenant
$tenantId = "00000000-0000-0000-0000-000000000000"
$subscriptionId = "00000000-0000-0000-0000-000000000000"

Connect-AzAccount -TenantId $tenantId -SubscriptionId $subscriptionId

$policyDefinition = New-AzPolicyDefinition -Name "Deny-Creation-Of-Azure-OpenAI" `
    -DisplayName "Deny creation of Azure OpenAI resources" `
    -Description "This policy denies the creation of Azure OpenAI resources." `
    -Policy '{
        "policyRule": {
            "if": {
                "field": "type",
                "equals": "Microsoft.CognitiveServices/accounts"
            },
            "then": {
                "effect": "deny"
            }
        }
    }' `
    -Mode "All"

# Assign to subscription
New-AzPolicyAssignment -Name "Deny-Creation-Of-Azure-OpenAI" `
-PolicyDefinition $policyDefinition `
-Scope "/subscriptions/{subscription-id}"

# Or assign to management group
New-AzPolicyAssignment -Name "Deny-Creation-Of-Azure-OpenAI" `
-PolicyDefinition $policyDefinition `
-Scope "/providers/Microsoft.Management/managementGroups/{management-group-id}"