# Newmazon

Amazon amerikai cég által használt autómatizált pakoló robotrendszer szimulációja, melyet Szoftvertechnológia tárgyhoz készítettem Ács Botond és Dóra Lászlóval együtt. 

## Leírás
Egy raktár tipikus alaprajza:

![newmazon](https://user-images.githubusercontent.com/66735724/84387521-6d74df80-abf3-11ea-889c-13f6e8da7b8f.PNG)

A raktár négyzethálóba van szervezve. Vannak benne akkumulátoros robotok (Ri), célállomások (Si),
dokkolónak nevezett töltőállomások (Di), és pod-nak nevezett állványok (P). A robotok
akkumulátorának van egy maximális töltöttségi állapota, ami egy egész szám. Az állványokon termékek
vannak, amit a fenti ábrán az állványra írt számok jelölnek. Egy állványon minden termékszámból
legfeljebb egy szerepelhet. A termékszámok a célállomás számok közül kerülnek ki. A robotok feladata,
hogy a termékeket az azonos számú célállomásokhoz vigyék (1-es termék az S1 célállomáshoz, 2-es az
S2-höz, stb.).

Egy robot úgy tud egy terméket elvinni, hogy az állvány alá áll, megemeli az állványt, majd utána az
állvánnyal együtt mozog. Egy adott számú termék akkor kerül a célállomásra, ha a robot ráállt az
állvánnyal az azonos számú célállomásra, majd a terméket leadja, és ezzel a termék eltűnik az
állványról. A termék (vagy termékek) célállomásra szállítása után a robotoknak az állványt vissza kell
vinniük az eredeti helyükre.

A robotnak be kell tartania a robotmozgás szabályait:
  - A robotok az állványok alatt el tudnak menni.
  - Egy négyzetben sohasem lehet egyszerre két robot.
  - Egy négyzetben sohasem lehet egyszerre két állvány.
  - A robotok állvánnyal nem állhatnak töltőállomásra.
  - Mindig gondoskodni kell arról, hogy a robot az akkumulátorának lemerülése előtt el tudjon
    jutni egy töltőállomásra.
    
A szimuláció lépésekben történik. Egy központi vezérlő jelöli ki, hogy melyik robot melyik terméket
vigye el a célállomására. Tervezéstől függően, a robotok vagy önállóan dolgoznak, vagy egy központi
vezérlő mondja meg, hogy melyik lépésben milyen műveletet végezzenek. A központi vezérlő és a
robotok is az egész raktár állapotát ismerik. Minden lépésben minden robot végezhet egy műveletet,
ami (a termék leadás műveletét leszámítva) eggyel csökkenti az akkumulátor töltöttségi állapotát.
A lehetséges műveletek:
  - kilencven fokos fordulat jobbra vagy balra,
  - átlépés a menetirány szerinti él-szomszédos négyzetbe (végrehajtható, ha induláskor a cél
    négyzetben nincs se robot se fal, és betartja a robotmozgás szabályait),
  - állvány megemelése (végrehajtható, ha az állvánnyal egy mezőben áll, és innentől együtt
    mozognak),
  - állvány letevése (innentől az állvány nem mozdul, csak a robot mozoghat),
  - termék leadása (sikeres, ha megfelelő célállomáson áll, ez a művelet nem csökkenti az
    akkumulátor töltöttségét),
   - akkumulátor töltésének elindítása (végrehajtható, ha töltőállomáson áll, és akkor utána még
     öt lépésig a robot nem mozdulhat a töltőállomásról, ami alatt teljesen feltöltődik az
     akkumulátora).
     
A szimuláció indításához meg kell adni:
   - a raktár elrendezését (méretek, állványok, célállomások, dokkolók helye)
  - robotok száma és helye, maximális töltöttség (tegyük fel, hogy kezdetben minden robot
    teljesen feltöltött)
  - kiszállítandó termékek helye (melyik állványon vannak)
  - (esetleg egyéb konfigurációs adatok, amik segíthetik a megoldást)
  
Az állítható sebességű szimuláció során szeretnénk látni a raktár alaprajzát, a robotok mozgását, a
robotok telítettségi állapotát, a termékeket, az állványokat és mozgásukat.

A szimuláció végén egy napló fájlban meg akarjuk kapni:
  - hány lépésig tartott a teljes feladat végrehajtása,
  - minden egyes robotra, hogy összesen mennyi energiát fogyasztott,
  - összesen mennyi energia kellett a feladat végrehajtásához.

Videó a robotok valós életbeli falhsználásáról:
https://www.youtube.com/watch?v=Ox05Bks2Q3s

