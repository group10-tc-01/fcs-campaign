# language: pt-BR
Funcionalidade: Listagem de campanhas
  Como cliente da API
  Quero consultar campanhas de forma paginada
  Para navegar pelos resultados de maneira previsível

  Cenário: Listar campanhas administrativas com metadados de paginação
    Dado que estou autenticado como "GestorONG"
    E existem 3 campanhas com status "Active"
    Quando eu listar a página 2 com tamanho 2
    Então a resposta deve ter status 200
    E a página retornada deve ser 2 com tamanho 2 e total 3
    E a listagem deve conter 1 item

  Cenário: Normalizar paginação inválida
    Dado que estou autenticado como "GestorONG"
    E existem 12 campanhas com status "Active"
    Quando eu listar a página 0 com tamanho 101
    Então a resposta deve ter status 200
    E a página retornada deve ser 1 com tamanho 10 e total 12
    E a listagem deve conter 10 itens

  Cenário: Filtrar por múltiplos status sem duplicar filtros
    Dado que estou autenticado como "GestorONG"
    E existem 2 campanhas com status "Active"
    E existem 1 campanhas com status "Completed"
    E existem 1 campanhas com status "Canceled"
    Quando eu listar filtrando pelos status "Active,Completed,Active"
    Então a resposta deve ter status 200
    E a página retornada deve ser 1 com tamanho 10 e total 3
    E a listagem deve conter somente os status "Active,Completed"

  Cenário: Listar todas as campanhas sem filtro
    Dado que estou autenticado como "GestorONG"
    E existem 1 campanhas com status "Active"
    E existem 1 campanhas com status "Completed"
    E existem 1 campanhas com status "Canceled"
    Quando eu listar as campanhas administrativas
    Então a resposta deve ter status 200
    E a página retornada deve ser 1 com tamanho 10 e total 3

  Cenário: Transparência é pública, paginada e contém somente campanhas ativas
    Dado que não estou autenticado
    E existem 2 campanhas com status "Active"
    E existem 1 campanhas com status "Completed"
    Quando eu consultar a transparência
    Então a resposta deve ter status 200
    E a página retornada deve ser 1 com tamanho 10 e total 2
    E a listagem de transparência deve conter somente campanhas ativas

