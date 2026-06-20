# language: pt-BR
Funcionalidade: Gestão de campanhas
  Como GestorONG
  Quero administrar campanhas
  Para manter as iniciativas de arrecadação atualizadas

  Cenário: Criar campanha sempre como ativa
    Dado que estou autenticado como "GestorONG"
    Quando eu criar uma campanha válida
    Então a resposta deve ter status 201
    E a campanha retornada deve estar com status "Active"
    E a campanha deve registrar o gestor autenticado
    E deve ser publicada a auditoria "CampaignCreated"

  Esquema do Cenário: Rejeitar criação de campanha inválida
    Dado que estou autenticado como "GestorONG"
    Quando eu criar uma campanha com "<campo>" inválido
    Então a resposta deve ter status 400

    Exemplos:
      | campo       |
      | titulo      |
      | descricao   |
      | data final  |
      | intervalo   |
      | meta         |

  Cenário: Impedir acesso administrativo sem autenticação
    Dado que não estou autenticado
    Quando eu listar as campanhas administrativas
    Então a resposta deve ter status 401

  Cenário: Impedir acesso administrativo sem a role GestorONG
    Dado que estou autenticado como "Doador"
    Quando eu listar as campanhas administrativas
    Então a resposta deve ter status 403

  Cenário: Editar campanha ativa
    Dado que estou autenticado como "GestorONG"
    E existe uma campanha com status "Active"
    Quando eu editar a campanha
    Então a resposta deve ter status 200
    E a campanha deve conter os dados atualizados
    E deve ser publicada a auditoria "CampaignUpdated"

  Cenário: Não editar campanha encerrada
    Dado que estou autenticado como "GestorONG"
    E existe uma campanha com status "Completed"
    Quando eu editar a campanha
    Então a resposta deve ter status 400

  Cenário: Retornar não encontrado ao editar campanha inexistente
    Dado que estou autenticado como "GestorONG"
    Quando eu editar uma campanha inexistente
    Então a resposta deve ter status 404

  Cenário: Consultar campanha por identificador
    Dado que estou autenticado como "GestorONG"
    E existe uma campanha com status "Active"
    Quando eu consultar a campanha por identificador
    Então a resposta deve ter status 200
    E a campanha consultada deve ser a campanha existente

  Cenário: Retornar não encontrado ao consultar campanha inexistente
    Dado que estou autenticado como "GestorONG"
    Quando eu consultar uma campanha inexistente
    Então a resposta deve ter status 404

  Esquema do Cenário: Alterar status de campanha ativa
    Dado que estou autenticado como "GestorONG"
    E existe uma campanha com status "Active"
    Quando eu alterar o status da campanha para "<status>"
    Então a resposta deve ter status 200
    E o status retornado deve ser "<status>"
    E deve ser publicada a auditoria "<auditoria>"

    Exemplos:
      | status    | auditoria         |
      | Completed | CampaignCompleted |
      | Canceled  | CampaignCanceled  |

  Esquema do Cenário: Rejeitar transição de status inválida
    Dado que estou autenticado como "GestorONG"
    E existe uma campanha com status "<inicial>"
    Quando eu alterar o status da campanha para "<destino>"
    Então a resposta deve ter status 400

    Exemplos:
      | inicial   | destino   |
      | Active    | Active    |
      | Completed | Active    |
      | Completed | Canceled  |
      | Canceled  | Active    |
      | Canceled  | Completed |

  Cenário: Rejeitar status desconhecido
    Dado que estou autenticado como "GestorONG"
    E existe uma campanha com status "Active"
    Quando eu alterar o status da campanha para "Unknown"
    Então a resposta deve ter status 400
    E a resposta deve informar que os status válidos são "Completed, Canceled"
