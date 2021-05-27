using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchHelper2
{
    class Artefacts
    {
        static string artefactsHardCodedSingle = @"Centurion's dress sword
1 5 2 5
250
Venator dagger
3 16 4 12
305.1
Venator light crossbow
3 12 4 16
305.1
Legionary gladius
3 10 4 6 6 12
430.8
Legionary square shield
3 8 4 8 6 12
430.8
Primis Elementis standard
5 16 3 12
430.8
Zaros effigy
5 8 7 10 4 12
520.5
Zarosian training dummy
3 16 7 14
520.5
Hookah pipe
3 10 8 12 9 8
574.4
Opulent wine goblet
3 14 8 16
574.4
Crest of Dagon
8 14 9 18
646.2
'Disorder' painting
5 6 7 6 10 6 11 14
646.2
Legatus Maximus figurine
8 8 4 14 12 10
664.1
'Solem in Umbra' painting
5 8 7 10 13 14
664.1
Imp mask
14 10 15 10 16 12
735.9
Lesser demon mask
14 6 15 8 16 12 11 6
735.9
Greater demon mask
3 6 14 6 15 8 16 12
735.9
Order of Dis robes
5 16 11 10 17 14
861.5
Ritual dagger
8 16 18 24 38 1
861.5
'Frying pan'
3 20 20 24
1073.3
Hallowed lantern
3 20 19 24 39 1
1073.3
Branding iron
3 14 17 12 18 20
1283.3
Manacles
3 14 15 18 17 14
1283.3
Ancient timepiece
8 12 6 16 12 18
1423.3
Legatus pendant
3 16 8 18 12 12 40 1
1423.3
Ceremonial unicorn ornament
19 26 21 20
1493.3
Ceremonial unicorn saddle
14 24 21 22
1493.3
Tetracompass(unpowered)
53 1 54 1 55 1 56 1 29 30 11 30 21 30 28 30 13 30
2065
Everlight harp
22 30 7 22
1703.3
Everlight trumpet
22 28 8 24
1703.3
Everlight violin
23 16 7 20 5 16
1703.3
Folded-arm figurine(female)
20 30 8 24
2053.3
Folded-arm figurine(male)
20 30 8 24
2053.3
Pontifex signet ring
3 16 8 18 12 22 40 1
2193.3
'Incite Fear' spell scroll
10 20 12 18 24 18
2193.3
Dominion discus
19 34 23 28
2566.7
Dominion javelin
19 32 3 30
2566.7
Dominion pelte shield
23 34 5 28
2566.7
'The Lake of Fire' painting
5 10 7 10 10 10 11 34
3500
'Lust' metal sculpture
3 16 17 24 8 24 38 1
3500
Chaos star
15 28 18 36
4200
Spiked dog collar
3 24 14 24 15 16
4200
Bronze Dominion medal
22 36 23 26 41 1
4433.3
Silver Dominion medal
22 36 23 26 42 1
4433.3
Dominion torch
8 12 9 12 22 20 23 18
4433.3
Ikovian gerege
3 36 26 30
4666.7
Toy glider
25 36 7 30
4666.7
Toy war golem
3 36 7 30 43 1
4666.7
Decorative vase
20 36 21 30
5133.3
Patera bowl
19 36 8 30 44 1
5133.3
Kantharos cup
22 30 9 36 44 1
5133.3
Ceremonial mace
6 20 3 20 8 28
5600
'Consensus ad Idem' painting
7 10 5 10 13 50
5600
Pontifex Maximus figurine
4 24 12 16 8 28 40 1
5600
Avian song-egg player
25 36 28 32 45 1
6066.7
Keshik drum
26 16 27 16 7 20 14 16
6066.7
Morin khuur
28 36 7 32
6066.7
Ekeleshuun blinder mask
31 24 29 20 10 24
6066.7
Narogoshuun 'Hob-da-Gob' ball
31 36 30 32
6066.7
Rekeshuun war tether
32 20 31 22 14 26
6066.7
Aviansie dreamcoat
28 20 5 30 27 22
7388.9
Ceremonial plume
28 38 8 34 46 1
7388.9
Peacocking parasol
28 22 5 30 7 20
7388.9
Ogre Kyzaj axe
32 28 30 20 33 24
7388.9
Ork cleaver sword
32 36 33 36
7388.9
Larupia trophy
11 18 27 28 9 26
7388.9
Lion trophy
11 18 27 28 7 26
7388.9
She-wolf trophy
15 26 11 18 27 28
7388.9
Pontifex censer
3 20 12 20 8 32 40 1
7388.9
Pontifex crozier
6 20 4 20 8 32
7388.9
Pontifex mitre
5 32 12 20 4 20
7388.9
Thorobshuun battle standard
30 16 29 22 7 16 5 20
8166.7
Yurkolgokh stink grenade
34 38 31 36 47 1
8166.7
Dominarian device
22 30 19 22 3 22 43 1
8555.6
Fishing trident
23 22 3 30 8 22
8555.6
Hawkeye lens multi-vision scope
25 40 9 34
8944.4
Talon-3 razor wing
35 40 26 34 48 1
8944.4
Necromantic focus
6 20 24 26 12 30
9333.3
'Exsanguinate' spell scroll
10 40 24 36
9333.3
High priest crozier
30 26 29 24 8 28
10500
High priest mitre
30 26 29 24 5 28
10500
High priest orb
30 26 29 24 8 28
10500
'Pandemonium' tapestry
7 12 5 12 10 12 11 42
10500
'Torment' metal sculpture
17 20 3 20 18 38
10500
Prototype gravimeter
36 34 14 20 3 26
11277.8
Songbird recorder
25 44 9 36 45 1
11277.8
Amphora
22 34 19 46
11666.7
Rod of Asclepius
20 30 23 24 8 26
11666.7
Zarosian ewer
3 52 4 30
12500
Zarosian stein
3 16 6 36 4 30
12500
Beastkeeper helm
32 16 31 24 27 20 33 24
13333.3
Idithuun horn ring
34 40 31 44
13333.3
'Nosorog!' sculpture
34 30 29 24 32 30
13333.3
Stormguard gerege
25 36 26 28 8 20
14166.7
Dayguard shield
25 36 26 28 7 20
14166.7
Garagorshuun anchor
32 32 30 26 3 30
15833.3
Ourg megahitter
7 20 14 20 9 26 29 22
15833.3
Ourg tower/goblin cower shield
30 20 3 26 14 22 7 20
15833.3
Golem heart
35 34 36 24 9 16 37 16
16666.7
Golem instruction
36 46 10 44 49 1
16666.7
Hellfire haladie
18 44 3 26 14 20
16666.7
Hellfire katar
18 50 14 40
16666.7
Hellfire zaghnal
18 38 7 26 9 26
16666.7
Dorgeshuun spear
32 50 7 42
18666.7
'Forged in War' sculpture
32 50 34 42 50 1
18666.7
Kopis dagger
22 50 14 42
18666.7
Xiphos short sword
22 46 14 46
18666.7
'Smoke Cloud' spell scroll
10 40 12 20 24 32
18666.7
Vigorem vial
6 54 12 38 51 1
18666.7
Blackfire lance
35 50 36 46
22166.7
Nightguard shield
25 30 26 36 7 30
22166.7
Huzamogaarb chaos crown
32 44 3 34 17 20
23333.3
Saragorgak star crown
32 44 3 34 23 20
23333.3
'Possession' metal sculpture
17 24 15 30 3 44
23333.3
Trishula
18 48 17 30 3 20
23333.3
Tsutsaroth piercing
18 44 15 30 11 24
23333.3
'The Pride of Padosan' painting
21 52 7 16 5 16 10 16
24500
'Hallowed Be the Everlight' painting
21 52 7 16 5 16 10 16
24500
'The Lord of Light' painting
21 52 7 16 5 16 10 16
24500
Ancient magic tablet
12 40 24 64
27000
Portable phylactery
6 48 24 36 12 20
27000
'Animate Dead' spell scroll
10 40 12 24 24 40
27000
'The Enlightened Soul' scroll
23 50 10 60
29666.7
'The Eudoxian Elements' tablet
20 60 8 50
29666.7
Drogokishuun hook sword
32 44 29 36 33 32
31000
Hobgoblin mansticker
32 66 33 46
31000
Chaos Elemental trophy
15 52 7 30 18 30
31000
Virius trophy
16 44 7 34 9 34
31000
Flat cap
28 60 5 54
32333.3
Night owl flight goggles
28 44 14 40 9 30
32333.3
Prototype godbow
35 50 36 34 26 34
33666.7
Prototype godstaff
35 50 36 34 26 34
33666.7
Prototype godsword
35 50 26 34 8 34
33666.7
Praetorian hood
12 36 5 48 4 40 52 30
36666.7
Praetorian robes
12 30 5 54 4 40 52 50
36666.7
Praetorian staff
6 36 12 58 4 30 52 100
36666.7
Kal-i-kra chieftain crown
34 66 27 60
38333.3
Kal-i-kra mace
31 42 3 44 33 40
38333.3
Kal-i-kra warhorn
31 44 33 42 27 40
38333.3
Spear of Annhilation
31 500 29 500 8 500
38333.3
Tsutsaroth helm
18 50 17 40 8 40
40000
Tsutsaroth pauldron
18 40 8 50 17 40
40000
Tsutsaroth urumi
18 50 17 40 3 40
40000
Kontos lance
22 70 5 62
41666.7
Doru spear
22 70 7 62
41666.7
Chuluu stone
35 40 36 30 37 40 8 24
43333.3
Quintessence counter
36 54 25 40 7 40
43333.3
Spherical astrolabe
35 46 28 40 9 48
43333.3
Ancient globe
7 20 13 54 12 60
43333.3
Battle plans
10 40 13 60 12 34
43333.3
'Prima Legio' painting
7 20 5 20 13 74 4 20
43333.3
Horogothgar cooking pot
34 60 29 38 37 40
45000
'Da Boss Man' sculpture
34 50 29 44 37 44
45000";

        public static string[] artefactsHardCoded = artefactsHardCodedSingle.Split("\r\n");
    }
}
