# FiapCloudGames.AzureFunction

## 📌 Objetivos
Microsserviço de pagamentos do Monólito [FiapCloudGames](https://github.com/MarioGuilherme/FiapCloudGames.AzureFunction) que trata todas as regras e lógicas pertinente ao escopo de jogos, compras, pedidos e recomendações de jogos, juntamente com o sua base de dados, com integração com ElasticSearch e recomendações e pesquisas inteligentes de jogos pelo usuário e seu histório de compra.

## 🚀 Instruções de uso
Faça o clone do projeto e já acesse a pasta do projeto clonado:
```
git clone https://github.com/MarioGuilherme/FiapCloudGames.AzureFunction && cd .\FiapCloudGames.AzureFunctions
```

### ▶️ Iniciar Projeto
  1 - Navegue até o diretório da camada API da aplicação:
  ```
  cd .\FiapCloudGames.AzureFunctions.Functions\
  ```
  2 - Insira o comando de execução do projeto:
  ```
  dotnet run
  ```

  3 - Acesse https://localhost:7147/swagger/index.html

## 🛠️ Tecnologias e Afins
- Azure Function Isolated Model;
- Logs Distribuídos com CorrelationId;
- Uso de Middlewares e IActionFilterS;
- EntityFrameworkCore;
- SQL SERVER;
- Swagger;
