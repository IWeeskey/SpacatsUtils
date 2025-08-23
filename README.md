![Screenshot](Arts/SpaCats%20LogoSmall.png)

Spacats Utils is a lightweight collection of essential utilities and helper tools used across various Spacats projects.

Main features:
- Controller hub + controller logic
- Simple MonoTween system with pause, chaining, repeat, and step logic

MonoTween Controller tests:
1) Device: Android TechnoSpark 8C (Android 11)
- Spawning 1k tweens instantly takes about 1.4–7ms
- Running 1k tweens at the same time costs 0.4ms
- At around 40k tweens there is no any single frame loss
2) On PC… testing feels pointless — you can push around 50k tweens at the same time with no loss, but honestly, you’ll probably never need that many unless you’re summoning the Tweenpocalypse.

INSTALLATION
1) Open Unity and go to the top menu:
2) Window → Package Manager.
3) In the Package Manager window, click the "+" button in the top-left corner.
4) Select "Add package from git URL..." from the dropdown.
5) Paste the GitHub repository link:
https://github.com/IWeeskey/SpacatsUtils.git
6) Click "Add" and wait for Unity to download and install the package.
7) Once installed, you can find the scripts and assets inside your Packages folder in Unity. Also you can check out the examples.

Should work on any unity versions above 2019
Tested on Unity Versions:
2022.3.39f1
2023.2.20f1
