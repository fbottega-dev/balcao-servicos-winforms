# Entendendo o Balcão de Serviços

O problema é simples: uma assistência precisa saber quais equipamentos estão aguardando atendimento, o que já começou e como cada serviço terminou. O projeto acompanha esse fluxo em uma aplicação desktop.

Antes de apresentar o código, execute o programa e percorra uma ordem completa. Assim, a explicação parte de uma situação que você já viu funcionar.

## Um roteiro de cinco minutos

1. Abra o aplicativo com `--demo` e veja as situações na lista.
2. Cadastre um cliente e abra uma ordem para um notebook que não carrega.
3. Selecione a ordem e inicie o atendimento.
4. Conclua com um valor e uma observação sobre o reparo.
5. Abra o histórico e confira o problema original, a abertura, o início e a conclusão.
6. Cadastre outra ordem e cancele com um motivo. Compare os dois históricos.
7. Busque pelo cliente e aplique um filtro de situação.
8. Selecione uma ordem, edite o nome do cliente e confira que a correção aparece em todas as ordens dele.

Explique que o modo demonstração perde os dados ao fechar. Para demonstrar persistência, execute sem `--demo`, usando o SQL Server, feche e reabra o programa.

## O caminho de uma operação

Use **Iniciar atendimento** como exemplo e acompanhe estes arquivos:

1. [MainForm.cs](../src/Balcao.WinForms/MainForm.cs) dispara o evento `IniciarSolicitado` quando o botão é acionado.
2. [AtendimentoPresenter.cs](../src/Balcao.Apresentacao/AtendimentoPresenter.cs) recebe o evento, confere a seleção e chama o serviço.
3. [ServicoAtendimento.cs](../src/Balcao.Dominio/Servicos/ServicoAtendimento.cs) carrega a ordem e verifica se ela está aberta. Só então muda a situação para `EmAndamento`.
4. [RepositorioSqlServer.cs](../src/Balcao.Dados/RepositorioSqlServer.cs) grava a alteração e acrescenta o histórico com o EF6.
5. O presenter atualiza a lista e a tela informa que o atendimento começou.

A regra fica no serviço porque precisa valer mesmo que outra tela venha a chamar a mesma operação. O botão desabilitado ajuda o usuário, mas a validação continua existindo fora da interface.

No WinForms, trocar a fonte da tabela pode selecionar a primeira linha automaticamente. `MainForm.ExibirOrdens` limpa essa seleção depois da busca e só restaura o ID escolhido antes quando ele ainda aparece na lista. Assim, filtrar ordens não habilita ações para outro atendimento por acidente.

Na edição do cliente, o presenter usa o ID da ordem para localizar o cliente. O serviço valida os campos antes de salvar. A lista consulta novamente o repositório e mostra o nome corrigido em todas as ordens do mesmo cliente; os registros de atendimento continuam ligados ao mesmo ID.

## Onde aparecem os fundamentos de C#

| Conceito | Exemplo concreto |
| --- | --- |
| Classe | `Cliente` guarda os dados do cliente; `OrdemServico` representa o atendimento. |
| Herança | `ClienteForm` e outros cadastros herdam de `FormularioBase`, que monta os campos e botões compartilhados. |
| Interface | `IRepositorioAtendimento` define as operações que o serviço usa para acessar os dados. |
| Polimorfismo | `RepositorioMemoria` e `RepositorioSqlServer` implementam esse mesmo contrato. |
| Coleções e condições | A listagem usa coleções; as mudanças de situação dependem de validações com `if`. |
| Exceções | `RegraNegocioException` indica uma operação inválida; `ConflitoEdicaoException` indica uma versão desatualizada. |

O MVP é a separação entre a tela, o presenter e os dados/regras usados por ele. `IAtendimentoView` descreve o que o presenter precisa da tela. Os testes podem substituir a janela por uma implementação simples dessa interface.

## Banco e EF6

[AtendimentoContext.cs](../src/Balcao.Dados/AtendimentoContext.cs) relaciona as classes às tabelas. O esquema está escrito em SQL para deixar visíveis as chaves, tipos e restrições. As consultas de estudo estão em [database/consultas.sql](../database/consultas.sql).

- `Clientes`: uma linha por cliente.
- `OrdensServico`: equipamento, problema, situação, datas e valor final; `ClienteId` aponta para o cliente.
- `HistoricosOrdem`: acontecimentos de cada ordem; `OrdemId` aponta para a ordem.

O EF6 traduz as consultas LINQ e acompanha as alterações que precisam ser salvas. Ele usa parâmetros nas consultas. O projeto usa EF6, a linha adequada a esta estrutura de .NET Framework, e não EF Core.

`rowversion` é um valor gerado pelo SQL Server a cada alteração da linha. Se uma operação tentar salvar a versão antiga, o EF percebe que ela já mudou. A aplicação pede uma atualização em vez de sobrescrever o registro. O teste de integração reproduz esse caso com duas leituras da mesma ordem.

A ordem e seu histórico precisam concordar. Por isso, a gravação da mudança de situação e do histórico acontece na mesma transação: se falhar, a operação não deve ficar pela metade.

## O que os testes ajudam a conferir

Em [ServicoAtendimentoTests.cs](../tests/Balcao.Tests/ServicoAtendimentoTests.cs), procure os cenários de conclusão, cancelamento e validação. Rode a suíte e altere uma regra de propósito para observar um teste falhar; depois desfaça a alteração.

Em [AtendimentoPresenterTests.cs](../tests/Balcao.Tests/AtendimentoPresenterTests.cs), observe a `TelaFalsa`. Ela permite verificar seleção, filtros e tratamento de cadastro inválido sem abrir o WinForms.

Em [RepositorioSqlServerTests.cs](../tests/Balcao.Integracao/RepositorioSqlServerTests.cs), a verificação acontece no banco real. Esses testes complementam os testes em memória: o repositório em memória não prova que o mapeamento do EF e o SQL estão corretos.

## Por que a estrutura parece de um projeto antigo?

A solução usa .NET Framework 4.8, C# 5, `packages.config`, `.csproj` tradicional e MSBuild. O objetivo é praticar restauração de pacotes, referências e compilação fora do `dotnet CLI`, como pode acontecer na manutenção de aplicações desktop existentes.

O projeto em si é novo. O contato com essa estrutura é um exercício prático, não experiência profissional com um sistema legado de produção.

## Uma próxima mudança pequena

Escolha uma melhoria que você consiga justificar, como validar melhor o telefone opcional. Defina exemplos válidos e inválidos, ajuste o serviço, acrescente testes e rode o build. Abra um pull request explicando o comportamento antes e depois.

Na apresentação, mostre essa mudança e o teste correspondente. Entender uma decisão pequena e conseguir modificá-la é mais útil do que decorar o nome de todos os padrões do projeto.
