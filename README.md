[![Build](https://github.com/jonathandbailey/maf-stateless-worflow-checkpoints/actions/workflows/dotnet.yml/badge.svg)](https://github.com/jonathandbailey/maf-stateless-worflow-checkpoints/actions/workflows/dotnet.yml)

**Educational project exploring Microsoft Agent Framework and Agentic patterns.**

> ⚠️ **Work in Progress / Educational Project**  
> This repository is a learning and experimentation space — not a production-ready framework.  
> APIs, structure, and agents may change frequently.
> 

## Open AI Azure Setup
- This example uses an Azure OpenAI resource.
- The Model on Azure Foundry : gpt-4o-mini (version: 2024-07-18) 
- 4o-mini is used to produce structured JSON with the Microsfot Agent Framework
- To connect and authenticate you need to provide a 'DeploymentName' & 'Endpoint' in the appSettings (development)
- The application is using DefaultAzureCredential so no API key is required.

## Agent Templates and User/Session Data
- The application is using Azurite (Local Azure Storage Emulator) created by Aspire in a container.
- A local bind mount perisists the data between runs
- You can use Microsoft Azure Storage Explorer to view the local storage instance (needs to be running)
- The life cycle of the application is managed by a session id , which is used by the console client while the application runs.