# language: pt-BR
Funcionalidade: Integração de doações com campanhas
  Como serviço interno
  Quero validar e refletir doações
  Para manter o total arrecadado consistente

  Cenário: Campanha ativa é elegível para doação
    Dado que existe uma campanha com status "Active"
    Quando eu consultar a elegibilidade da campanha
    Então a resposta deve ter status 200
    E a campanha deve estar elegível

  Esquema do Cenário: Campanha encerrada não é elegível
    Dado que existe uma campanha com status "<status>"
    Quando eu consultar a elegibilidade da campanha
    Então a resposta deve ter status 200
    E a campanha não deve estar elegível

    Exemplos:
      | status    |
      | Completed |
      | Canceled  |

  Cenário: Campanha inexistente não possui elegibilidade
    Quando eu consultar a elegibilidade de uma campanha inexistente
    Então a resposta deve ter status 404

  Cenário: Refletir doação processada
    Dado que existe uma campanha com status "Active"
    Quando eu refletir uma doação de 250
    Então a resposta deve ter status 200
    E o total arrecadado deve ser 250
    E deve ser publicada a auditoria "DonationReflected"

  Cenário: Rejeitar doação com valor não positivo
    Dado que existe uma campanha com status "Active"
    Quando eu refletir uma doação de 0
    Então a resposta deve ter status 400

  Cenário: Rejeitar doação em campanha encerrada
    Dado que existe uma campanha com status "Completed"
    Quando eu refletir uma doação de 100
    Então a resposta deve ter status 400

  Cenário: Ignorar doação duplicada
    Dado que existe uma campanha com status "Active"
    Quando eu refletir a mesma doação de 100 duas vezes
    Então ambas as respostas devem ter status 200
    E o total arrecadado deve ser 100
    E a segunda resposta deve indicar duplicidade
    E deve ser publicada a auditoria "DuplicateDonationIgnored"

