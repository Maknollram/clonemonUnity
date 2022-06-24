### Mudar de scenes (cenários):
* Crie uma scenes
* Add no build settings do projeto no Unity
* Na scene anterior no sceneToLoad do portal coloque o número da scene que add no build project
* Use o prefab Portal e set x ou y do spawnPoint
* Add o prefab essentialObjectsLoader a scene
* Selecione o portal conforme desejar

### Ler scenes ao mesmo tempo (fazer abaixo ou duplicar o existente)
* Crie uma scene principal (gameplay) e Add EssentialObjectLoader
* Add novas scenes criadas pelo comando open scene additive, remova os portais caso tenham, ou crie novas scenes e add pelo comando open scene additive, add a nova scene no build settings do projeto
* Coloque o mesmo nome da scene criada ou movida e adicione BoxCollider2D e o ajeite do tamanho da scene
* Defina em ConnectScenes quais scenes serão carregadas junto com a que o player estiver
