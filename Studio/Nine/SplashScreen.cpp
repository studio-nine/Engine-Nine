#include <windows.h>
#include <metahost.h>
#include <strsafe.h>
#include "Resource.h"

#pragma comment(lib, "mscoree.lib")

HRESULT RuntimeHost(PCWSTR pszVersion, PCWSTR pszAssemblyPath, PCWSTR pszClassName, PCWSTR pszStaticMethodName, PCWSTR pszStringArg)
{
    HRESULT hr;
    DWORD dwLengthRet;
    ICLRMetaHost *pMetaHost = NULL;
    ICLRRuntimeInfo *pRuntimeInfo = NULL;
    ICLRRuntimeHost *pClrRuntimeHost = NULL;
    ICorRuntimeHost *pCorRuntimeHost = NULL;
    
    hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&pMetaHost));
    if (FAILED(hr))
        goto Cleanup;
    
    hr = pMetaHost->GetRuntime(pszVersion, IID_PPV_ARGS(&pRuntimeInfo));
    if (FAILED(hr))
        goto Cleanup;
    
    BOOL fLoadable;
    hr = pRuntimeInfo->IsLoadable(&fLoadable);
    if (FAILED(hr) || !fLoadable)
        goto Cleanup;

    hr = pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&pClrRuntimeHost));
    if (FAILED(hr))
        goto Cleanup;
    
    hr = pClrRuntimeHost->Start();
    if (FAILED(hr))
        goto Cleanup;

    hr = pClrRuntimeHost->ExecuteInDefaultAppDomain(pszAssemblyPath, pszClassName, pszStaticMethodName, pszStringArg, &dwLengthRet);
    if (FAILED(hr))
        goto Cleanup;

Cleanup:
    if (pMetaHost)
        pMetaHost->Release();
    if (pRuntimeInfo)
        pRuntimeInfo->Release();
    if (pClrRuntimeHost)
        pClrRuntimeHost->Release();

    return hr;
}


HBITMAP hbm;
int screenWidth;
int screenHeight;
int logoHeight = 200;

LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    if (WM_DESTROY == message)
    {
        PostQuitMessage(0);
        return 0;
    }

    if (WM_PAINT == message)
    {   
        PAINTSTRUCT ps;
        HDC hdc = BeginPaint(hWnd, &ps);

        HDC hdcMem = CreateCompatibleDC(NULL);
        HBITMAP hbmT = (HBITMAP)SelectObject(hdcMem, (HGDIOBJ)hbm);

        BITMAP bm;
        GetObject(hbm,sizeof(bm),&bm);

        BitBlt(hdc, (screenWidth - bm.bmWidth) / 2, (logoHeight - bm.bmHeight) / 2, bm.bmWidth, bm.bmHeight, hdcMem, 0, 0, SRCCOPY);
        
        SelectObject(hdcMem, (HGDIOBJ)hbm);
        DeleteDC(hdcMem);

        EndPaint(hWnd, &ps);
        return 0;
    }

    return DefWindowProc(hWnd, message, wParam, lParam);
}

void CreateSplash(HINSTANCE hInstance, LPWSTR lpCmdLine)
{
    WNDCLASSEX wcex;
    ZeroMemory(&wcex, sizeof(WNDCLASSEX));

    wcex.cbSize = sizeof(WNDCLASSEX);
    wcex.style = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc = WndProc;
    wcex.hbrBackground = CreateSolidBrush(RGB(95, 120, 157));
    wcex.hInstance = hInstance;
    wcex.lpszClassName = L"Nine";

    RegisterClassEx(&wcex);

    screenWidth = GetSystemMetrics(SM_CXVIRTUALSCREEN);
    screenHeight = GetSystemMetrics(SM_CYVIRTUALSCREEN);

    hbm = LoadBitmap(hInstance, MAKEINTRESOURCE(IDB_LOGO));

    HWND window = CreateWindowEx(WS_EX_TOPMOST | WS_EX_TOOLWINDOW, L"Nine", L"", WS_DISABLED | WS_POPUP | WS_VISIBLE, 0, (screenHeight - logoHeight) / 2, screenWidth, logoHeight, NULL, NULL, hInstance, NULL);

    ShowWindow(window, SW_SHOWNORMAL);
    UpdateWindow(window);

    size_t cbCmdLine;
    StringCbLength(lpCmdLine, STRSAFE_MAX_CCH * sizeof(TCHAR), &cbCmdLine);
    wchar_t *commandLine = new wchar_t[lstrlen(lpCmdLine)];
    StringCbPrintf(commandLine, cbCmdLine + MAX_PATH, L"%s %d", lpCmdLine, (int)window);
    
    CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);

    wchar_t* executable = (0xFFFFFFFF != GetFileAttributes(L"Bin/Nine.Studio.Shell.dll")) ? L"Bin/Nine.Studio.Shell.dll" : 
                         ((0xFFFFFFFF != GetFileAttributes(L"Bin/Nine.Studio.Shell.exe")) ? L"Bin/Nine.Studio.Shell.exe" : L"Nine.Studio.Shell.exe");
    
    RuntimeHost(L"v4.0.30319", executable, L"Nine.Studio.Shell.BootStrapper", L"Run", commandLine);
}

int APIENTRY wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, int nCmdShow)
{
    CreateSplash(hInstance, lpCmdLine);
    return 0;
}