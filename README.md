#  Jogo baseado em Taiko no Tatsujin
Feito por Artur Oyakawa e Ian Ushiwata

# Como jogar
As notas vermelhas se chamam **Don**, as azuis **Katsu**.

É necessário apertar uma das teclas correspondentes à nota assim que a mesma chegar no círculo cinza (no rítmo da música)

## Keybinds
- **Z** => Katsu
- **X** => Don
- **C** => Don
- **V** => Katsu

Notas grandes necessitam que as duas teclas sejam pressionadas para que o acerto seja contado. 

Por exemplo, para acertar um **Large Don** (Don grande), é necessário apertar as teclas **X** e **C** ao mesmo tempo.

Para acertar um **Large Katsu** (Katsu grande), por sua vez, é preciso apertar **Z** e **V** ao mesmo tempo.

## Judgements:
- **Ryo!** => Hit no timing certo. (Acerto dentro de uma janela de 42ms)
- **Ka.** => Perto o suficiente. (Acerto dentro de uma janela de 108ms)
- **Fuka.** => Miss ou muito longe. (Acerto dentro de uma janela de 180ms ou a nota passou direto)

# A maioria das coisas podem ser alteradas diretamente no inspector, destacam-se:
### Lane:
- Scroll Speed (velocidade das notas na tela) = 17.9f;
- Song Speed (velocidade da música) = 1f;
- Chart Name (o nome da música que será jogada. alterações não serão registradas em runtime) = Executioner;
- Offset (delay em milissegundos das teclas em relação a música a fim de compensar a latência nos dispositivos de áudio) = 83.

## Judgement:
- AutoPlay (joga a chart automaticamente, acertando sempre Ryo!'s) = false.

## Charts disponíveis atualmente
- Tempestissimo;
- Executioner;
- LagTrain;
- BigBlack.
