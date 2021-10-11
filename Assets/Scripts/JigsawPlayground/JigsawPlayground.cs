using Assets.Scripts.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class JigsawPlayground : MonoBehaviour
{
    public static JigsawPlayground Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Seviye bilgisi.
    /// </summary>
    public LevelEditorModel LevelData { get; private set; }

    [Header("Sonland�r�ld� m�? Oyun bir �ekilde bitti ise art�k tutma i�lemini ve di�er i�lemleri bitirece�iz.")]
    public bool IsFinalized;

    [Header("T�klamalar� hesaplamak i�in kullan�lacak ��z�n�rl�k.")]
    public Canvas PlaygroundCanvas;

    [Header("Hint controller.")]
    public LevelHintController LHC;

    [Header("Oyun alan�na yerle�tirilecek puzzle par�as�.")]
    public GameObject JigsawPlaygroundItem;

    [Header("Par�alar�n yerle�tirilece�i alan.")]
    public RectTransform JigsawPlaygroundContent;

    [Header("Par�alar�n �izimleri.")]
    public Sprite[] Pieces;

    [Header("Toplam sat�r say�s�.")]
    public int RowCount;

    [Header("Toplam s�tun say�s�.")]
    public int ColCount;

    [Header("Her par�a aras�ndaki fark.")]
    public float PerOffset;

    [Header("Par�alar�n olu�turulaca�� alan.")]
    public RectTransform SpawnArea;

    [Header("Seviyeye ait par�alar.")]
    public List<JigsawPlaygroundItemController> LevelPieces;

    [Header("Se�imi yap�lan ana par�a.")]
    public JigsawPlaygroundItemController SelectedPiece;

    [Header("Se�ili olan par�alar. Ana par�a da dahil.")]
    public List<JigsawPlaygroundItemController> SelectedPieces;

    /// <summary>
    /// We use the generator to generate same random level.
    /// </summary>
    public System.Random Randomizer { get; set; }

    /// <summary>
    /// Mouse position to drag and drop.
    /// </summary>
    private Vector2 selectedPieceStartMPos { get; set; }

    private void Start()
    {
        PlaygroundCanvas = GameObject.FindGameObjectWithTag("GUI").GetComponent<Canvas>();
    }

    public void LoadLevel(LevelEditorModel levelData, Texture2D levelTexture)
    {
        // We create a randomizer.
        Randomizer = levelData.GetRandom();

        // Seviye bilgisini at�yoruz.
        this.LevelData = levelData;

        // Par�a sayac�.
        int i = 0;

        // Oyun alan�n boyutlar�.
        Rect sizeOfContent = JigsawPlaygroundContent.GetComponent<RectTransform>().rect;

        // Sat�rlar� d�n�yoruz.
        for (int r = RowCount - 1; r >= 0; r--)
        {
            // S�tunlar� d�n�yoruz.
            for (int c = 0; c < ColCount; c++)
            {
                // Par�a ikonunu buluyoruz.
                Sprite piece = Pieces[i];

                // Konumland�rma.
                float posX = -sizeOfContent.width / 2 + PerOffset * c + 23.75f;
                float posY = -sizeOfContent.height / 4f + PerOffset * r + 23.75f;

                // Par�alar� olu�turuyoruz.
                GameObject jigsawItem = Instantiate(JigsawPlaygroundItem, JigsawPlaygroundContent);
                jigsawItem.GetComponent<RectTransform>().anchoredPosition = GetGivenPosition(c, r);

                // �izimi ve konumland�rmas�n� yap�yoruz.
                jigsawItem.transform.Find("Puzzle").GetComponent<Image>().sprite = piece;
                jigsawItem.transform.Find("Outline").GetComponent<Image>().sprite = piece;

                // Par�an�n g�r�nt�lenece�i alana� hespal�yoruz..
                RawImage pieceBack = jigsawItem.transform.Find("Puzzle/Back").GetComponent<RawImage>();
                pieceBack.texture = levelTexture;
                pieceBack.GetComponent<RectTransform>().anchoredPosition = GetGivenPosition(c, r) * -1;
                pieceBack.SetNativeSize();

                // �ndeksi bir artt�r�yoruz bir sonraki par�ay� s�ras�yla y�klemek i�in.
                i++;

                // Kom�ular�n� belirliyoruz.
                JigsawPlaygroundItemController jigsawItemComp = jigsawItem.GetComponent<JigsawPlaygroundItemController>();
                jigsawItemComp.SetGridData(r, c);

                // Par�alar� listeye ekliyoruz.
                LevelPieces.Add(jigsawItemComp);
            }
        }

        // Rastgele s�ra ile s�ralamalar�n� g�ncelliyoruz.
        foreach (JigsawPlaygroundItemController levelPiece in LevelPieces.OrderBy(x => Randomizer.NextDouble()))
        {
            // UI s�ralamas�n� g�ncelliyoruz rastgele olarak.
            levelPiece.GetComponent<Canvas>().sortingOrder = 1;
        }

    }

    public void CheckForNeighborConnections()
    {
        // Maksimum etkile�im mesafesi.
        float interactionOffset = PerOffset * 1.25f;

        // Etkile�im mesafesi. En az olabilecek mesafe.
        float interactionSide = interactionOffset * .5f;

        // Etkile�im kar�� kenar mesafe kontorl�. �rn; sa� ve sol kom�u i�in bu de�er �st ve alt komu�lar�n kontrol� i�in kullan�lacak.
        float interactionReverseSide = interactionOffset * .15f;

        // �lk ba�lant�.
        JigsawPlaygroundItemController firstConnection = null;

        // T�m se�imlerin kom�ular�n� kontrol ediyoruz.
        foreach (JigsawPlaygroundItemController sPiece in this.SelectedPieces.OrderByDescending(x => x == this.SelectedPiece))
        {
            /** Kom�ular� al�yoruz. **/
            JigsawPlaygroundItemController rightNeighbor = GetRightNeighbor(sPiece.Row, sPiece.Column);
            JigsawPlaygroundItemController leftNeighbor = GetLeftNeighbor(sPiece.Row, sPiece.Column);
            JigsawPlaygroundItemController topNeighbor = GetTopNeighbor(sPiece.Row, sPiece.Column);
            JigsawPlaygroundItemController bottomNeighbor = GetBottomNeighbor(sPiece.Row, sPiece.Column);

            // Sa� taraftaki kom�usu.
            if (rightNeighbor)
            {
                // �ki par�a aras�ndaki mesafeleri al�yoruz.
                float xDiff = rightNeighbor.RectTransform.anchoredPosition.x - sPiece.RectTransform.anchoredPosition.x;
                float yDiff = Mathf.Abs(rightNeighbor.RectTransform.anchoredPosition.y - sPiece.RectTransform.anchoredPosition.y);

                // Mesafe etkile�im mesafesinden az ise bu iki birim birbirlerine dokunuyor demektir.
                if (Vector2.Distance(sPiece.RectTransform.anchoredPosition, rightNeighbor.RectTransform.anchoredPosition) <= interactionOffset)
                {
                    // Ancak kontrol etmemiz gereken bu iki birimden birisi di�erine sa�dan m� dokunuyor?
                    // Ayn� zamanda yukar� ve a�a��dan da mesafeyi �l��yoruz ki uzak mesafelerde de birbirlerine ba�lanmas�nlar.
                    if (xDiff >= interactionSide && xDiff <= interactionOffset && yDiff >= 0 && yDiff <= interactionReverseSide)
                    {
                        // Par�alar� birbirlerine ba�l�yorlar.
                        sPiece.ConnectToPiece(rightNeighbor);

                        // �lk ba�lan�lan par�a.
                        if (firstConnection == null)
                            firstConnection = rightNeighbor;
                    }
                }
            }

            // Sol taraftaki kom�usu.
            if (leftNeighbor)
            {
                // �ki par�a aras�ndaki mesafeleri al�yoruz.
                float xDiff = sPiece.RectTransform.anchoredPosition.x - leftNeighbor.RectTransform.anchoredPosition.x;
                float yDiff = Mathf.Abs(sPiece.RectTransform.anchoredPosition.y - leftNeighbor.RectTransform.anchoredPosition.y);

                // Mesafe etkile�im mesafesinden az ise bu iki birim birbirlerine dokunuyor demektir.
                if (Vector2.Distance(sPiece.RectTransform.anchoredPosition, leftNeighbor.RectTransform.anchoredPosition) <= interactionOffset)
                {
                    // Ancak kontrol etmemiz gereken bu iki birimden birisi di�erine soldan m� dokunuyor?
                    // Ayn� zamanda yukar� ve a�a��dan da mesafeyi �l��yoruz ki uzak mesafelerde de birbirlerine ba�lanmas�nlar.
                    if (xDiff >= interactionSide && xDiff <= interactionOffset && yDiff >= 0 && yDiff <= interactionReverseSide)
                    {
                        // Par�alar� birbirlerine ba�l�yorlar.
                        sPiece.ConnectToPiece(leftNeighbor);

                        // �lk ba�lan�lan par�a.
                        if (firstConnection == null)
                            firstConnection = leftNeighbor;
                    }
                }
            }

            // �st kom�usunu kontrol ediyoruz.
            if (topNeighbor)
            {
                // �ki par�a aras�ndaki mesafeleri al�yoruz.
                float xDiff = Mathf.Abs(sPiece.RectTransform.anchoredPosition.x - topNeighbor.RectTransform.anchoredPosition.x);
                float yDiff = sPiece.RectTransform.anchoredPosition.y - topNeighbor.RectTransform.anchoredPosition.y;

                // Mesafe etkile�im mesafesinden az ise bu iki birim birbirlerine dokunuyor demektir.
                if (Vector2.Distance(sPiece.RectTransform.anchoredPosition, topNeighbor.RectTransform.anchoredPosition) <= interactionOffset)
                {
                    // Ancak kontrol etmemiz gereken bu iki birimden birisi di�erine yukar�dan m� dokunuyor?
                    // Ayn� zamanda sa�dan ve soldan da mesafeyi �l��yoruz ki uzak mesafelerde de birbirlerine ba�lanmas�nlar.
                    if (yDiff >= interactionSide && yDiff <= interactionOffset && xDiff >= 0 && xDiff <= interactionReverseSide)
                    {
                        // Par�alar� birbirlerine ba�l�yorlar.
                        sPiece.ConnectToPiece(topNeighbor);

                        // �lk ba�lan�lan par�a.
                        if (firstConnection == null)
                            firstConnection = topNeighbor;
                    }
                }
            }

            // Alt komu�usunu kontrol ediyoruz.
            if (bottomNeighbor)
            {
                // �ki par�a aras�ndaki mesafeleri al�yoruz.
                float xDiff = Mathf.Abs(bottomNeighbor.RectTransform.anchoredPosition.x - sPiece.RectTransform.anchoredPosition.x);
                float yDiff = bottomNeighbor.RectTransform.anchoredPosition.y - sPiece.RectTransform.anchoredPosition.y;

                // Mesafe etkile�im mesafesinden az ise bu iki birim birbirlerine dokunuyor demektir.
                if (Vector2.Distance(sPiece.RectTransform.anchoredPosition, bottomNeighbor.RectTransform.anchoredPosition) <= interactionOffset)
                {
                    // Ancak kontrol etmemiz gereken bu iki birimden birisi di�erine a�a��dan m� dokunuyor?
                    // Ayn� zamanda sa�dan ve soldan da mesafeyi �l��yoruz ki uzak mesafelerde de birbirlerine ba�lanmas�nlar.
                    if (yDiff >= interactionSide && yDiff <= interactionOffset && xDiff >= 0 && xDiff <= interactionReverseSide)
                    {
                        // Par�alar� birbirlerine ba�l�yorlar.
                        sPiece.ConnectToPiece(bottomNeighbor);

                        // �lk ba�lan�lan par�a.
                        if (firstConnection == null)
                            firstConnection = bottomNeighbor;
                    }
                }
            }
        }

        // E�er ba�lant� yok ise kendisini se�iyoruz.
        if (firstConnection == null)
            firstConnection = this.SelectedPiece;

        // T�m par�alar� yeniden konumland�r�yoruz.
        foreach (JigsawPlaygroundItemController piece in this.SelectedPieces.OrderByDescending(x => x == firstConnection))
            piece.RectTransform.anchoredPosition = firstConnection.RectTransform.anchoredPosition + GetPositionConnectedJigsawPuzzle(firstConnection.Row, firstConnection.Column, piece.Row, piece.Column);
    }

    public void ContinueToLevel()
    {
        // We get the history data.
        GameHistoryModel historyValue = GameHistoryController.Instance.GetHistory(this.LevelData, GameDifficulities.JigsawPuzzle);

        // We loop all the items.
        foreach (GameHistoryPieceModel piece in historyValue.HistoryPieces)
        {
            // We are looking for the same piece from previous level.
            JigsawPlaygroundItemController pieceInLevel = LevelPieces.Find(x => x.Row == piece.Row && x.Column == piece.Col);

            // We make sure piece exists in map.
            if (pieceInLevel != null)
            {
                // We load the piece position from the cache.
                pieceInLevel.RectTransform.anchoredPosition = new Vector2(piece.X, piece.Y);
            }
        }

        // We check all the neighbors for all the pieces.
        historyValue.HistoryPieces.ForEach(e =>
        {
            // We are looking for the same piece from previous level.
            JigsawPlaygroundItemController pieceInLevel = LevelPieces.Find(x => x.Row == e.Row && x.Column == e.Col);

            // We make sure piece exists.
            if (pieceInLevel != null)
            {
                // We select the piece.
                OnSelectPiece(pieceInLevel);

                // And check for its connections.
                CheckForNeighborConnections();

                // We deselect piece.
                OnDeselectPiece();
            }
        });
    }

    public JigsawPlaygroundItemController GetRightNeighbor(int pieceRow, int pieceCol)
    {
        return LevelPieces.Find(x => x.Row == pieceRow && x.Column == pieceCol + 1);
    }
    public JigsawPlaygroundItemController GetTopNeighbor(int pieceRow, int pieceCol)
    {
        return LevelPieces.Find(x => x.Row == pieceRow - 1 && x.Column == pieceCol);
    }
    public JigsawPlaygroundItemController GetBottomNeighbor(int pieceRow, int pieceCol)
    {
        return LevelPieces.Find(x => x.Row == pieceRow + 1 && x.Column == pieceCol);
    }
    public JigsawPlaygroundItemController GetLeftNeighbor(int pieceRow, int pieceCol)
    {
        return LevelPieces.Find(x => x.Row == pieceRow && x.Column == pieceCol - 1);
    }
    public Vector2 GetPositionConnectedJigsawPuzzle(int frow, int fcol, int trow, int tcol)
    {
        int rowDiff = trow - frow;
        int colDiff = tcol - fcol;
        return new Vector2(colDiff * PerOffset, rowDiff * PerOffset);
    }

    public void SelectReleatedPieces(JigsawPlaygroundItemController piece)
    {
        // T�m ba�l� olan par�alar� d�n�yoruz.
        piece.ConnectedJigsaws.ForEach(e =>
        {
            // E�er se�im listesinde yok ise ekleyece�iz.
            if (!SelectedPieces.Contains(e))
            {
                // Se�im listesine ekliyoruz.
                SelectedPieces.Add(e);

                // VE bu ba�l� olan par�an�n di�er ba�l� olan par�alar�n� d�n�yoruz.
                SelectReleatedPieces(e);
            }
        });
    }
    public void OnSelectPiece(JigsawPlaygroundItemController piece)
    {
        // E�er sonland�r�lm�� ise geri d�n.
        if (IsFinalized)
            return;

        // Se�imleri tutuyoruz.
        this.SelectedPiece = piece;
        this.selectedPieceStartMPos = Input.mousePosition;

        // �nceki listenini temizlendi�inden emin oluyoruz.
        SelectedPieces.Clear();

        // Se�ili par�alar� listeye ekliyoruz.
        SelectedPieces.Add(piece);

        // Se�ilen birim ile ba�l� olan t�m par�alar� se�iyoruz.
        SelectReleatedPieces(piece);

        // T�m se�imlerin s�ralamas�n� yukar� al�yoruz ki par�alar altta kalmas�n.
        SelectedPieces.ForEach(e => e.GetComponent<Canvas>().sortingOrder = 2);
    }
    public void OnDeselectPiece()
    {
        // E�er se�im yok ise geri d�n�yoruz.
        if (this.SelectedPiece == null)
            return;

        // Komu�ular ile ba�lant�lar� olanlar� kontrol ediyoruz.
        CheckForNeighborConnections();

        // T�m se�imlerin s�ralamas�n� yukar� al�yoruz ki par�alar altta kalmas�n.
        SelectedPieces.ForEach(e => e.GetComponent<Canvas>().sortingOrder = 1);

        // Se�im listesini temizliyoruz.
        this.SelectedPieces.Clear();

        // Ve se�imi kald�r�yoruz.
        this.SelectedPiece = null;

        // Bir ad�m azalt�yoruz ve kay�t ediyoruz ve devam�nda ui yenileniyor.
        SaveLoadController.Instance.SaveData.ActionScore--;
        SaveLoadController.Instance.Save();
        CurrentLevelGameViewController.Instance.RefreshUI();

        // Level pieces.
        List<GameHistoryPieceModel> pieces = LevelPieces.Select(x => new GameHistoryPieceModel(x.RectTransform.anchoredPosition.x, x.RectTransform.anchoredPosition.y, x.Column, x.Row))
            .ToList();

        // We remove history if the level completed.
        GameHistoryController.Instance.SetHistory(this.LevelData, GameDifficulities.JigsawPuzzle, pieces);

        // Oyun sonunu kontrol ediyoruz.
        int totalCombinedPiece = LevelPieces.SelectMany(x => x.ConnectedJigsaws.Select(y => y)).Distinct().Count();

        // E�er toplam adete eri�iyor ise.
        if (totalCombinedPiece == LevelPieces.Count)
            FinalizePlayground();
    }

    private void FinalizePlayground()
    {
        // E�er sonland�r�lmam�� ise sonland�r�yoruz.
        if (!IsFinalized)
        {
            // Sonland�r�ld� olarak i�aretliyoruz.
            IsFinalized = true;

            // Current level information.
            LevelEditorModel currentLevelData = CurrentLevelGameViewController.Instance.LevelData;

            // We are updating max level if it is smaller.
            if (CurrentLevelGameViewController.Instance.CurrentLevel == SaveLoadController.Instance.SaveData.MaxReachedLevel)
                SaveLoadController.Instance.SaveData.MaxReachedLevel = CurrentLevelGameViewController.Instance.CurrentLevel + 1;

            // We are reducing one point.
            SaveLoadController.Instance.SaveData.ActionScore += currentLevelData.ScoreOnWin;

            // We are applying changes.
            SaveLoadController.Instance.Save();

            // And we are reducing from the ui.
            CurrentLevelGameViewController.Instance.RefreshUI();

            // We are updating buttons states.
            CurrentLevelGameViewController.Instance.CheckPrevAndNextButtons();

            // We remove history if the level completed.
            GameHistoryController.Instance.RemoveHistory(this.LevelData, GameDifficulities.JigsawPuzzle);
        }
    }

    private void LateUpdate()
    {
        // E�er bir par�aya ba�l� ise.
        if (Input.GetMouseButton(0))
        {
            // E�er adet kalmad�ysa geri d�n.
            if (this.SelectedPieces.Count > 0)
            {
                // Farenin bir �nceki konumu ile aras�ndaki fark.
                Vector2 diff = (Vector2)Input.mousePosition - selectedPieceStartMPos;

                // Bu de�er asl�nda canvas scale ratesi. Kaymalar� �nlemek ad�na ayn� oranda olmal�.
                diff.x /= PlaygroundCanvas.transform.localScale.x;
                diff.y /= PlaygroundCanvas.transform.localScale.y;

                // Sa� taraf s�n�r kontorl� yap�yoruz.
                if (diff.x > 0)
                {
                    // Ta��nan par�alardan en u�takini buluyoruz.
                    float mostRightPieceX = SelectedPieces.Select(x => x.RectTransform.anchoredPosition.x + PerOffset / 2).Max();

                    // Ve bir sonraki konumunu hesapl�yoruz.
                    mostRightPieceX += diff.x;

                    // Sa� s�n�r.
                    float rightBorder = JigsawPlaygroundContent.anchoredPosition.x + JigsawPlaygroundContent.rect.max.x;

                    // E�er ekran�n d���na ��kar ise 0 l�yoruz.
                    if (mostRightPieceX > rightBorder)
                        diff.x += rightBorder - mostRightPieceX;
                }

                // Sol taraf s�n�r kontrol�n� yap�yoruz.
                if (diff.x < 0)
                {
                    // En soldaki par�ay� al�yoruz.
                    float mostLeftPieceX = SelectedPieces.Select(x => x.RectTransform.anchoredPosition.x - PerOffset / 2).Min();

                    // En ucun yeni konumunu hesaplad�k.
                    mostLeftPieceX += diff.x;

                    // Sol s�n�r.
                    float leftBorder = JigsawPlaygroundContent.anchoredPosition.x + JigsawPlaygroundContent.rect.min.x;

                    // E�er ekran�n d���na ��kar ise 0 l�yoruz.
                    if (mostLeftPieceX < leftBorder)
                        diff.x += leftBorder - mostLeftPieceX;
                }

                // E�er yukar�ya do�ru ilerlemeye �al���yor isek s�n�r�n� kontrol ediyoruz.
                if (diff.y > 0)
                {
                    // En yukar�daki par�ay� bulduk.
                    float mostTopPieceY = SelectedPieces.Select(x => x.RectTransform.anchoredPosition.y + PerOffset / 2).Max();

                    // Bir sonraki konumunu hesaplad�k.
                    mostTopPieceY += diff.y;

                    // �st s�n�r.
                    float topBorder = JigsawPlaygroundContent.anchoredPosition.y + JigsawPlaygroundContent.rect.max.y;

                    // E�er ekran�n d���na ��kar ise 0 l�yoruz.
                    if (mostTopPieceY > topBorder)
                        diff.y += topBorder - mostTopPieceY;
                }

                // E�er a�a�� do�ru ilerlemeye �al���yor isek s�n�r�n� kontrol ediyoruz.
                if (diff.y < 0)
                {
                    // En yukar�daki par�ay� bulduk.
                    float mostBottomPieceY = SelectedPieces.Select(x => x.RectTransform.anchoredPosition.y - PerOffset / 2).Min();

                    // Bir sonraki konumunu hesaplad�k.
                    mostBottomPieceY += diff.y;

                    // Alt s�n�r.
                    float bottomBorder = JigsawPlaygroundContent.anchoredPosition.y + JigsawPlaygroundContent.rect.min.y;

                    // E�er ekran�n d���na ��kar ise 0 l�yoruz.
                    if (mostBottomPieceY < bottomBorder)
                        diff.y += bottomBorder - mostBottomPieceY;
                }

                // T�m se�ili olan par�alar�n konumunu g�ncelliyoruz.
                foreach (JigsawPlaygroundItemController piece in SelectedPieces)
                    piece.RectTransform.anchoredPosition += diff;

                // Ekrana g�re konumu g�ncelliyoruz sapma olmas�n diye.
                diff.x *= PlaygroundCanvas.transform.localScale.x;
                diff.y *= PlaygroundCanvas.transform.localScale.y;

                // Farenin son konumunu g�ncelliyoruz.
                selectedPieceStartMPos += diff;
            }
        }

        // E�er se�ili ise ve mouse b�rak�ld� ise se�imi kald�r�yoruz.
        if (Input.GetMouseButtonUp(0))
        {
            // Se�imi kald�rd�k.
            JigsawPlayground.Instance.OnDeselectPiece();
        }
    }

    public Vector2 GetRandomPosition()
    {
        // Rastgele x koordinat alan i�erisinde.
        float x = SpawnArea.rect.min.x + (float)Randomizer.NextDouble() * (SpawnArea.rect.max.x - SpawnArea.rect.min.x);

        // Rastgele y koordinat� verilen alan i�erisinde.
        float y = SpawnArea.rect.min.y + (float)Randomizer.NextDouble() * (SpawnArea.rect.max.y - SpawnArea.rect.min.y);

        // Alan� geri d�n�yoruz.
        return new Vector2(x, SpawnArea.rect.position.y - y);
    }

    public Vector2 GetGivenPosition(int col, int row)
    {
        // Rastgele x koordinat alan i�erisinde.
        float x = -(PerOffset * ColCount) / 2 + col * PerOffset + PerOffset / 2;

        // Rastgele y koordinat� verilen alan i�erisinde.
        float y = -(PerOffset * RowCount) / 2 + row * PerOffset + PerOffset / 2;

        // Alan� geri d�n�yoruz.
        return new Vector2(x, y);
    }

}
