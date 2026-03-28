Скрипты для второго программиста
================================

Папка:
Scripts/Programmer2_API

Файлы:
- BattleLaunchData.cs
- BattleLaunchButton.cs

Как использовать:
1. На сцене должно быть:
   - HubRoot
   - BattleRoot
   - объект с AutoBattleController
   - объект BattleLaunchButtonHost

2. На BattleLaunchButtonHost повесить BattleLaunchButton.

3. В инспекторе BattleLaunchButton назначить:
   - Hub Root
   - Battle Root
   - Auto Battle Controller

4. На кнопке старта из хаба:
   OnClick -> BattleLaunchButton.LaunchFromInspector()

5. На кнопке выхода из боя:
   OnClick -> BattleLaunchButton.ReturnToHub()

6. Данные боя можно:
   - выставлять вручную в инспекторе в Battle Data
   - или передавать кодом через:
     BattleLaunchButton.Launch(BattleLaunchData data)

Важное:
- Для инспекторного теста можно задать rank / manual stats / start hp / start qi / техники
- Если overrideStartHp / overrideStartQi выключены, бой стартует со стандартными значениями системы
