[![Build](https://github.com/jonathandbailey/maf-stateless-worflow-checkpoints/actions/workflows/dotnet.yml/badge.svg)](https://github.com/jonathandbailey/maf-stateless-worflow-checkpoints/actions/workflows/dotnet.yml)

**Educational project exploring Microsoft Agent Framework and Agentic patterns.**

> ⚠️ **Work in Progress / Educational Project**  
> This repository is a learning and experimentation space — not a production-ready framework.  
> APIs, structure, and agents may change frequently.


## The Application
- The application implements a Microsoft Agent Framework Workflow to allow the user to plan a vacation.
- A ReAct (Reason & Act) loop is used in the workflow to interact with the user and gather the required vacation details.
- A ReWoo (Reason without Observation) part of the workflow then uses an Orchestrator to delegate tasks to a Hotel, Flight, and Train workers.
- The application is designed to be extensible, so another worker, like an Accomodation Agent, can be added with minimal changes.

## Open AI Azure Setup
- Azure OpenAI Resource with Azure Foundry : gpt-4o-mini (version: 2024-07-18) 
- 4o-mini produces structured JSON with the Microsoft Agent Framework
- To connect and authenticate you need to provide a 'DeploymentName' & 'Endpoint' in the appSettings (development)
- DefaultAzureCredential so no API key is required.

## Agent Templates and User/Session Data
- Azurite (Local Azure Storage Emulator) created by Aspire in a container, a local bind mount perisists the data between runs
- You can use Microsoft Azure Storage Explorer to view the local storage instance (needs to be running)
- The life cycle of the application is managed by a session id , which is used by the console client while the application runs.